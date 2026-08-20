using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Helper;
using MobileWebApi.Constants;
using MobileWebApi.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;

namespace MobileWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IMeService _meService;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
        private readonly IMobileTenantConfigurationRepository _mobileTenantConfigurationRepository;
        private readonly IMobileModuleAccessService _mobileModuleAccessService;

        public AuthController(
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            ITokenService tokenService, 
            IOtpService otpService,
            IEmailService emailService,
            ISmsService smsService,
            IMeService meService,
            ILogger<AuthController> logger,
            IWebHostEnvironment environment,
            ITenantConfigurationRepository tenantConfigurationRepository,
            IMobileTenantConfigurationRepository mobileTenantConfigurationRepository,
            IMobileModuleAccessService mobileModuleAccessService)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _tokenService = tokenService;
            _otpService = otpService;
            _emailService = emailService;
            _smsService = smsService;
            _meService = meService ?? throw new ArgumentNullException(nameof(meService));
            _logger = logger;
            _environment = environment;
            _tenantConfigurationRepository = tenantConfigurationRepository;
            _mobileTenantConfigurationRepository = mobileTenantConfigurationRepository;
            _mobileModuleAccessService = mobileModuleAccessService;
        }

        /// <summary>
        /// Returns the currently authenticated user's profile and assigned work roles.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var profile = await _meService.GetCurrentUserAsync();
                if (profile == null)
                    return Unauthorized();

                return Ok(profile);
            }
            catch (TenantAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                int? userId = int.TryParse(userIdClaim, out var id) ? id : null;
                _logger.LogException(
                    ExceptionCodes.Me.GetCurrentUser,
                    nameof(GetCurrentUser),
                    ex,
                    userId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Username/Email and Password Login
        /// Authenticates user with Username or Email and Password
        /// POST: api/auth/login-email
        /// </summary>
        [HttpPost("login-email")]
        [ProducesResponseType(typeof(TokenWithRefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginWithEmail([FromBody] EmailLoginRequest request)
        {
            try
            {
                var usernameOrEmail = request?.GetUsernameOrEmail() ?? string.Empty;
                _logger.LogInformation(LogMessages.Auth.LoginAttempt, usernameOrEmail);

                if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(request?.password))
                {
                    _logger.LogWarning(LogMessages.Auth.LoginFailed, usernameOrEmail);
                    return BadRequest(new { Success = false, Message = AuthMessages.InvalidCredentials });
                }

                var user = await _userRepository.GetUserByUsernameOrEmailAsync(usernameOrEmail);

                // Generic failure for missing/inactive users — do not reveal which identifier failed.
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(LogMessages.Auth.LoginFailed, usernameOrEmail);
                    return Unauthorized(new { Success = false, Message = AuthMessages.InvalidCredentials });
                }

                // Validate password using the same process as Change Password and Reset Password
                bool isPasswordValid = ValidateUserPassword(request.password, user);
                if (!isPasswordValid)
                {
                    _logger.LogWarning(LogMessages.Auth.LoginFailed, usernameOrEmail);
                    return Unauthorized(new { Success = false, Message = AuthMessages.InvalidCredentials });
                }

				var tenantConfig = await _tenantConfigurationRepository
	  .GetByTenantIdAsync(
		  user.OrganisationId,
		  user.BranchId);
				var mobileTenantConfig = await _mobileTenantConfigurationRepository.GetByTenantIdAsync(user.OrganisationId);
				var moduleAccess = await _mobileModuleAccessService.GetModuleAccess(user.OrganisationId);
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(user.UserId);
                var authTokens = await _tokenService.GenerateTokensAsync(user);

                _logger.LogInformation(LogMessages.Auth.LoginSuccessful, usernameOrEmail);

                var isGeoFencingEnabled = tenantConfig?.IsGeoFencingEnabled ?? false;

                var workRoles = WorkRoleHelper.BuildLoginWorkRoles(
                    await _userRepository.GetActiveWorkRolesByUserIdAsync(user.UserId));

                var response = BuildLoginResponse(authTokens, user, tenantConfig, mobileTenantConfig, moduleAccess, isGeoFencingEnabled, employee, workRoles: workRoles);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.LoginWithEmail, nameof(LoginWithEmail), ex);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Mobile Login - Single unified endpoint for OTP-based authentication
        /// Handles three operations:
        /// 1. Send OTP: When only mobileNumber is provided (or otp is null/empty)
        /// 2. Verify OTP: When both mobileNumber and otp are provided
        /// 3. Resend OTP: Call again with only mobileNumber (respects 30s cooldown and 5 OTPs/hour limit)
        /// 
        /// OTP is stored in IMemoryCache (not database), hashed with SHA256, expires in 5 minutes.
        /// POST: api/auth/login-mobile
        /// </summary>
        [HttpPost("login-mobile")]
        [ProducesResponseType(typeof(TokenWithRefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MobileLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginMobile([FromBody] MobileLoginRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning(LogMessages.Otp.LoginMobileRequestNull);
                    return BadRequest(new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.RequestBodyRequired
                    });
                }

                if (string.IsNullOrWhiteSpace(request.mobileNumber))
                {
                    _logger.LogWarning(LogMessages.Otp.LoginMobileMobileNumberMissing);
                    return BadRequest(new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.MobileNumberRequired
                    });
                }

                // Validate mobile number format (10 digits)
                var normalizedMobile = NormalizeMobileNumber(request.mobileNumber);
                if (!IsValidMobileNumber(normalizedMobile))
                {
                    _logger.LogWarning(LogMessages.AuthAdditional.InvalidMobileNumberFormat, request.mobileNumber);
                    return BadRequest(new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.InvalidMobileNumberFormat
                    });
                }

                // Check if OTP is provided (not null and not empty)
                bool hasOtp = !string.IsNullOrWhiteSpace(request.otp);
                
                _logger.LogInformation(LogMessages.Otp.LoginMobileMobileHasOtp, 
                    MaskMobileNumber(normalizedMobile), hasOtp);

                // If OTP is provided, verify and login
                if (hasOtp)
                {
                    _logger.LogInformation(LogMessages.Otp.LoginMobileVerifyingOtp, MaskMobileNumber(normalizedMobile));
                    return await VerifyOtpAndLogin(normalizedMobile, request.otp!);
                }

                // If OTP is empty, send OTP
                _logger.LogInformation(LogMessages.Otp.LoginMobileSendingOtp, MaskMobileNumber(normalizedMobile));
                return await SendOtpForMobile(normalizedMobile);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.LoginMobile, nameof(LoginMobile), ex);
                return StatusCode(500, new MobileLoginResponse
                {
                    success = false,
                    message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Resend OTP to Mobile Number (DEPRECATED - Use login-mobile endpoint instead)
        /// This endpoint is kept for backward compatibility but is deprecated.
        /// Use POST /api/auth/login-mobile with only mobileNumber to resend OTP.
        /// POST: api/auth/resend-otp
        /// </summary>
        [HttpPost("resend-otp")]
        [Obsolete("Use POST /api/auth/login-mobile with only mobileNumber to resend OTP")]
        public async Task<IActionResult> ResendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.mobileNumber))
                {
                    return BadRequest(new SendOtpResponse
                    {
                        success = false,
                        message = OtpMessages.MobileNumberRequired
                    });
                }

                // Validate mobile number format (10 digits)
                var normalizedMobile = NormalizeMobileNumber(request.mobileNumber);
                if (!IsValidMobileNumber(normalizedMobile))
                {
                    return BadRequest(new SendOtpResponse
                    {
                        success = false,
                        message = OtpMessages.InvalidMobileNumberFormat
                    });
                }

                _logger.LogInformation(LogMessages.Otp.ResendOtpRequest, MaskMobileNumber(normalizedMobile));

                // Check if user exists in Users table by MobileNumber
                var user = await _userRepository.GetUserByMobileAsync(normalizedMobile);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(LogMessages.AuthAdditional.MobileNumberNotRegistered, MaskMobileNumber(normalizedMobile));
                    return BadRequest(new SendOtpResponse
                    {
                        success = false,
                        message = OtpMessages.MobileNumberNotRegistered
                    });
                }

                // Generate OTP with rate limiting and resend cooldown
                var (otp, resendAfterSeconds, canSend) = _otpService.GenerateMobileOtp(normalizedMobile);

                if (!canSend)
                {
                    if (resendAfterSeconds > 0)
                    {
                        return BadRequest(new SendOtpResponse
                        {
                            success = false,
                            message = resendAfterSeconds > 3600
                                ? OtpMessages.MaximumOtpLimitReached
                                : OtpMessages.PleaseWaitBeforeRequestingOtp,
                            resendAfterSeconds = resendAfterSeconds
                        });
                    }

                    return StatusCode(500, new SendOtpResponse
                    {
                        success = false,
                        message = OtpMessages.FailedToGenerateOtp
                    });
                }

                // Send OTP via SMS
                bool smsSent = await _smsService.SendOtpAsync(normalizedMobile, otp);

                if (!smsSent)
                {
                    // Remove OTP from cache if SMS failed
                    _otpService.RemoveMobileOtp(normalizedMobile);
                    _logger.LogError(LogMessages.Otp.FailedToSendSmsOtp, MaskMobileNumber(normalizedMobile));
                    return StatusCode(500, new SendOtpResponse
                    {
                        success = false,
                        message = OtpMessages.FailedToSendOtp
                    });
                }

                _logger.LogInformation(LogMessages.Otp.OtpResentSuccessfully, MaskMobileNumber(normalizedMobile));

                return Ok(new SendOtpResponse
                {
                    success = true,
                    message = OtpMessages.OtpSentSuccessfully,
                    resendAfterSeconds = resendAfterSeconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.ResendOtp, nameof(ResendOtp), ex);
                return StatusCode(500, new SendOtpResponse
                {
                    success = false,
                    message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Helper method to send OTP for mobile login
        /// </summary>
        private async Task<IActionResult> SendOtpForMobile(string normalizedMobile)
        {
            try
            {
                _logger.LogInformation(LogMessages.Otp.OtpSendRequest, MaskMobileNumber(normalizedMobile));

                // Check if user exists in Users table by MobileNumber
                var user = await _userRepository.GetUserByMobileAsync(normalizedMobile);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(LogMessages.AuthAdditional.MobileNumberNotRegistered, MaskMobileNumber(normalizedMobile));
                    return BadRequest(new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.MobileNumberNotRegistered
                    });
                }

                // Generate OTP with rate limiting and resend cooldown
                var (otp, resendAfterSeconds, canSend) = _otpService.GenerateMobileOtp(normalizedMobile);

                if (!canSend)
                {
                    if (resendAfterSeconds > 0)
                    {
                        return BadRequest(new MobileLoginResponse
                        {
                            success = false,
                            message = resendAfterSeconds > 3600
                                ? OtpMessages.MaximumOtpLimitReached
                                : OtpMessages.PleaseWaitBeforeRequestingOtp,
                            resendAfterSeconds = resendAfterSeconds
                        });
                    }

                    return StatusCode(500, new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.FailedToGenerateOtp
                    });
                }

                // Send OTP via SMS
                bool smsSent = await _smsService.SendOtpAsync(normalizedMobile, otp);

                if (!smsSent)
                {
                    // Remove OTP from cache if SMS failed
                    _otpService.RemoveMobileOtp(normalizedMobile);
                    _logger.LogError(LogMessages.Otp.FailedToSendSmsOtp, MaskMobileNumber(normalizedMobile));
                    return StatusCode(500, new MobileLoginResponse
                    {
                        success = false,
                        message = OtpMessages.FailedToSendOtp
                    });
                }

                _logger.LogInformation(LogMessages.Otp.OtpSentSuccessfully, MaskMobileNumber(normalizedMobile));

                // OTP should NEVER be returned in API response for security reasons
                // OTP is sent via SMS only
                return Ok(new MobileLoginResponse
                {
                    success = true,
                    message = OtpMessages.OtpSentSuccessfullyToMobile,
                    resendAfterSeconds = resendAfterSeconds,
                    otpSent = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.SendOtpForMobile, nameof(SendOtpForMobile), ex);
                return StatusCode(500, new MobileLoginResponse
                {
                    success = false,
                    message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

		/// <summary>
		/// Helper method to verify OTP and login
		/// </summary>
		private async Task<IActionResult> VerifyOtpAndLogin(string normalizedMobile, string otp)
		{
            try
            {
                // Validate OTP format (6 digits)
                if (otp.Length != 6 || !otp.All(char.IsDigit))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = OtpMessages.InvalidOtpFormat
                    });
                }

                _logger.LogInformation(LogMessages.Otp.OtpVerificationAttempt, MaskMobileNumber(normalizedMobile));

                // Check if user exists in Users table by MobileNumber
                var user = await _userRepository.GetUserByMobileAsync(normalizedMobile);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(LogMessages.AuthAdditional.MobileNumberNotRegistered, MaskMobileNumber(normalizedMobile));
                    return BadRequest(new
                    {
                        Success = false,
                        Message = OtpMessages.MobileNumberNotRegistered
                    });
                }

                // Validate OTP
                bool isValidOtp = _otpService.ValidateMobileOtp(normalizedMobile, otp);

                if (!isValidOtp)
                {
                    _logger.LogWarning(LogMessages.Otp.InvalidOtpForMobile, MaskMobileNumber(normalizedMobile));
                    return BadRequest(new
                    {
                        Success = false,
                        Message = OtpMessages.InvalidOrExpiredOtp
                    });
                }

                // Get Employee using SystemUserId from Users table
                Employee? employee = null;

                if (user.UserId > 0)
                {
                    employee = await _employeeRepository.GetEmployeebyUserIdAsync(user.UserId);
                }

                // Use Employee data if available, otherwise use User data
                int employeeId = employee?.Id ?? 0;
                int tenantId = employee?.OrganisationId ?? user.OrganisationId;
                int branchId = employee?.BranchId > 0 ? employee.BranchId : user.BranchId;

                if (branchId > 0)
                    user.BranchId = branchId;

                string name = employee?.Name ??
                              (!string.IsNullOrEmpty(employee?.FirstName)
                                  ? $"{employee?.FirstName} {employee?.LastName}".Trim()
                                  : user.DisplayName ?? user.Username);

                // Verify Employee is active if Employee record exists
                if (employee != null && !employee.IsEmployeeActive)
                {
                    _logger.LogWarning(LogMessages.AuthAdditional.EmployeeInactiveForMobile,
                        MaskMobileNumber(normalizedMobile), employee.Id);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = OtpMessages.EmployeeAccountInactive
                    });
                }

                // Get Tenant Configuration
                var tenantConfig = await _tenantConfigurationRepository
                    .GetByTenantIdAsync(tenantId,branchId);
                var mobileTenantConfig = await _mobileTenantConfigurationRepository.GetByTenantIdAsync(tenantId);
                var moduleAccess = await _mobileModuleAccessService.GetModuleAccess(tenantId);

                // Generate JWT + refresh token pair
                var authTokens = await _tokenService.GenerateTokensAsync(user);

                // Remove OTP from cache
                _otpService.RemoveMobileOtp(normalizedMobile);

                _logger.LogInformation(LogMessages.Otp.OtpVerifiedSuccessfully,
                    MaskMobileNumber(normalizedMobile), user.UserId, employeeId);

                var isGeoFencingEnabled = tenantConfig?.IsGeoFencingEnabled ?? false;

                var loginUser = new User
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    OrganisationId = tenantId
                };

                var workRoles = WorkRoleHelper.BuildLoginWorkRoles(
                    await _userRepository.GetActiveWorkRolesByUserIdAsync(user.UserId));

                return Ok(BuildLoginResponse(authTokens, loginUser, tenantConfig, mobileTenantConfig, moduleAccess, isGeoFencingEnabled, employee, tenantId, workRoles));
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.VerifyOtpAndLogin, nameof(VerifyOtpAndLogin), ex);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
		}
		/// <summary>
		/// Refresh access token using a valid refresh token.
		/// POST: api/auth/refresh-token
		/// Request: { "refreshToken": "..." }
		/// Response: { "accessToken": "..." }
		/// </summary>
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
		{
			try
			{
				_logger.LogInformation(LogMessages.Auth.RefreshTokenAttempt);

				if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
				{
					return BadRequest(new { Success = false, Message = AuthMessages.RefreshTokenRequired });
				}

				var response = await _tokenService.RefreshTokenAsync(request);

				_logger.LogInformation(LogMessages.Auth.RefreshTokenSuccessful, "User");

				return Ok(response);
			}
			catch (TokenRefreshException ex)
			{
				_logger.LogWarning(LogMessages.Auth.RefreshTokenInvalid);
				return Unauthorized(new { Success = false, Message = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogException(ExceptionCodes.Auth.RefreshToken, nameof(RefreshToken), ex);
				return StatusCode(500, new
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin
				});
			}
		}

		/// <summary>
		/// Logout - Revokes refresh token and optionally blacklists the access token from the Authorization header.
		/// POST: api/auth/logout
		/// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
        {
            try
            {
                var username = User.Identity?.IsAuthenticated == true
                    ? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown"
                    : "Unknown";

                _logger.LogInformation(LogMessages.Auth.LogoutAttempt, username);

                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(new LogoutResponse
                    {
                        Success = false,
                        Message = AuthMessages.RefreshTokenRequired
                    });
                }

                var revoked = await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
                if (!revoked)
                {
                    return Unauthorized(new LogoutResponse
                    {
                        Success = false,
                        Message = AuthMessages.InvalidRefreshToken
                    });
                }

                // Blacklist access token from Authorization header when present
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var accessToken = authHeader.Substring("Bearer ".Length).Trim();
                    var tokenHandler = new JwtSecurityTokenHandler();
                    if (tokenHandler.CanReadToken(accessToken))
                    {
                        var jwtToken = tokenHandler.ReadJwtToken(accessToken);
                        _tokenService.BlacklistToken(accessToken, jwtToken.ValidTo);
                        _logger.LogInformation(LogMessages.Auth.AccessTokenBlacklisted, username, jwtToken.Subject);
                    }
                }

                _logger.LogInformation(LogMessages.Auth.LogoutSuccessful, username);

                return Ok(new LogoutResponse
                {
                    Success = true,
                    Message = AuthMessages.LogoutSuccessful
                });
            }
            catch (Exception ex)
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                _logger.LogException(ExceptionCodes.Auth.Logout, nameof(Logout), ex);
                return StatusCode(500, new LogoutResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Forgot Password/PIN - Step 1: Request OTP
        /// Sends OTP to the registered email address or mobile number
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Auth.ForgotPasswordRequest, request.email);

                if (string.IsNullOrWhiteSpace(request.email))
                {
                    return BadRequest(new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.EmailOrMobileRequired
                    });
                }

                // Try to find user by email first, then by mobile number
                var user = await _userRepository.GetUserByEmailAsync(request.email);
                string identifier = request.email;

                if (user == null)
                {
                    // Try mobile number if email lookup failed
                    user = await _userRepository.GetUserByMobileAsync(request.email);
                    if (user != null)
                    {
                        identifier = user.MobileNumber ?? request.email;
                    }
                }

                if (user == null || !user.IsActive)
                {
                    return NotFound(new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.UserNotFoundOrInactive
                    });
                }

                // Validate that user has an email address for sending OTP
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = EmailMessages.EmailNotFoundForUser
                    });
                }

                // Generate OTP using the identifier (email or mobile)
                string otp = _otpService.GenerateOtp(identifier);
                _logger.LogInformation(LogMessages.Auth.OtpGenerated, identifier);

                // Send OTP via Email
                string userName = user.Username ?? user.Email;
                bool emailSent = await _emailService.SendForgotPasswordOtpAsync(user.Email, userName, otp);

                if (!emailSent)
                {
                    // Remove OTP if email failed to send
                    _otpService.RemoveOtp(identifier);
                    _logger.LogError(LogMessages.Email.FailedToSendOtpEmail, identifier);

                    return StatusCode(500, new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = EmailMessages.FailedToSendOtpEmail
                    });
                }

                string maskedContact = MaskContact(user.Email);

                return Ok(new ForgotPasswordResponse
                {
                    Success = true,
                    Message = AuthMessages.OtpSentSuccessfully,
                    SentTo = maskedContact
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.ForgotPassword, nameof(ForgotPassword), ex);
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Reset Password/PIN - Step 2: Verify OTP and set new password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Auth.OtpVerificationAttempt, request.email);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.email))
                {
                    return BadRequest(new { Success = false, Message = AuthMessages.EmailOrMobileRequired });
                }

                if (string.IsNullOrWhiteSpace(request.otp))
                {
                    return BadRequest(new { Success = false, Message = AuthMessages.OtpRequired });
                }

                if (string.IsNullOrWhiteSpace(request.new_password))
                {
                    return BadRequest(new { Success = false, Message = AuthMessages.NewPasswordRequired });
                }

                // Try to find user by email first, then by mobile number
                var user = await _userRepository.GetUserByEmailAsync(request.email);
                string identifier = request.email;

                if (user == null)
                {
                    // Try mobile number if email lookup failed
                    user = await _userRepository.GetUserByMobileAsync(request.email);
                    if (user != null)
                    {
                        identifier = user.MobileNumber ?? request.email;
                    }
                }

                if (user == null || !user.IsActive)
                {
                    return NotFound(new { Success = false, Message = AuthMessages.UserNotFoundOrInactive });
                }

                // Validate OTP
                if (!_otpService.ValidateOtp(identifier, request.otp))
                {
                    _logger.LogWarning(LogMessages.Auth.OtpVerificationFailed, identifier);
                    return BadRequest(new { Success = false, Message = AuthMessages.InvalidOtp });
                }

                // Generate new password hash and salt
                string salt = PasswordHelper.GenerateSalt();
                string passwordHash = PasswordHelper.HashPassword(request.new_password, salt);

                // Update password in database
                bool updated = await _userRepository.UpdatePasswordAsync(user.UserId, passwordHash, salt);

                if (updated)
                {
                    // Remove OTP after successful password reset
                    _otpService.RemoveOtp(identifier);
                    _logger.LogInformation(LogMessages.Auth.PasswordResetSuccessful, identifier);

                    // Send confirmation email
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        await _emailService.SendPasswordResetConfirmationAsync(user.Email, user.Username ?? user.Email);
                    }

                    return Ok(new { Success = true, Message = AuthMessages.PasswordResetSuccessful });
                }
                else
                {
                    _logger.LogError(LogMessages.Auth.PasswordResetFailed, identifier);
                    return StatusCode(500, new { Success = false, Message = AuthMessages.PasswordResetFailed });
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Auth.ResetPassword, nameof(ResetPassword), ex);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

      
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(LogMessages.Auth.LogoutUserIdClaimNotFound);
                    return Unauthorized(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.InvalidAuthenticationToken
                    });
                }

                _logger.LogInformation(LogMessages.Auth.ChangePasswordAttempt, username);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.current_password))
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.CurrentPasswordRequired
                    });
                }

                if (string.IsNullOrWhiteSpace(request.new_password))
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.NewPasswordRequired
                    });
                }

                // Get user from database
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return NotFound(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.UserNotFoundOrInactive
                    });
                }

                // Validate current password using the same process as login
                bool isCurrentPasswordValid = ValidateUserPassword(request.current_password, user);
                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning(LogMessages.Auth.CurrentPasswordIncorrect, username);
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.CurrentPasswordIncorrect
                    });
                }

                // Check if new password is different from current password
                // Use the same validation process as login
                bool isNewPasswordSame = ValidateUserPassword(request.new_password, user);
                if (isNewPasswordSame)
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.NewPasswordSameAsCurrent
                    });
                }

                // Generate new password hash and salt
                string salt = PasswordHelper.GenerateSalt();
                string passwordHash = PasswordHelper.HashPassword(request.new_password, salt);

                // Update password in database
                bool updated = await _userRepository.UpdatePasswordAsync(user.UserId, passwordHash, salt);

                if (updated)
                {
                    _logger.LogInformation(LogMessages.Auth.ChangePasswordSuccessful, username);

                    // Send confirmation email
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        await _emailService.SendPasswordResetConfirmationAsync(user.Email, user.Username ?? user.Email);
                    }

                    return Ok(new ChangePasswordResponse
                    {
                        Success = true,
                        Message = AuthMessages.PasswordChangeSuccessful
                    });
                }
                else
                {
                    _logger.LogError(LogMessages.Auth.ChangePasswordFailed, username);
                    return StatusCode(500, new ChangePasswordResponse
                    {
                        Success = false,
                        Message = AuthMessages.PasswordChangeFailed
                    });
                }
            }
            catch (Exception ex)
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                _logger.LogException(ExceptionCodes.Auth.ChangePassword, nameof(ChangePassword), ex);
                return StatusCode(500, new ChangePasswordResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }

        /// <summary>
        /// Builds the login response with access + refresh tokens and tenant metadata.
        /// </summary>
        private static TokenWithRefreshResponse BuildLoginResponse(
            AuthResponse authTokens,
            User user,
            TenantConfiguration? tenantConfig,
            MobileTenantConfiguration? mobileTenantConfig,
            MobileAccessDto? moduleAccess,
            bool isGeoFencingEnabled,
            Employee? employee = null,
            int? organisationIdOverride = null,
            IReadOnlyList<string>? workRoles = null)
        {
            var attendanceEnabled = mobileTenantConfig?.IsAttendanceEnabled ?? false;
            var tenantLocationTrackingEnabled = mobileTenantConfig?.EnableLocationTracking ?? false;
            var enableEmployeeLevelLocationTracking = mobileTenantConfig?.EnableEmployeeLevelLocationTracking ?? false;
            var employeeLocationTracking = employee?.EnableLocationTracking;

            var locationTracking = LocationTrackingSettingsHelper.Resolve(
                attendanceEnabled,
                tenantLocationTrackingEnabled,
                enableEmployeeLevelLocationTracking,
                employeeLocationTracking);

            // Hierarchical master-switch resolution:
            // 1. EnableLocationTracking is the master switch. If it is off, all
            //    employee-level flags are forced false and employee settings ignored.
            // 2. When on, EnableEmployeeLevelLocationTracking gates whether the
            //    employee's own EnableLocationTracking value is surfaced.
            var effectiveEnableLocationTracking = locationTracking.EnableLocationTracking;
            bool effectiveEnableEmployeeLevelLocationTracking;
            bool effectiveEmployeeLocationTrackingEnabled;
            if (!effectiveEnableLocationTracking)
            {
                effectiveEnableEmployeeLevelLocationTracking = false;
                effectiveEmployeeLocationTrackingEnabled = false;
            }
            else if (!enableEmployeeLevelLocationTracking)
            {
                effectiveEnableEmployeeLevelLocationTracking = false;
                effectiveEmployeeLocationTrackingEnabled = false;
            }
            else
            {
                effectiveEnableEmployeeLevelLocationTracking = true;
                effectiveEmployeeLocationTrackingEnabled = employeeLocationTracking ?? false;
            }

            return new TokenWithRefreshResponse
            {
                Success = true,
                Message = AuthMessages.TokenGenerated,
                AccessToken = authTokens.AccessToken,
                RefreshToken = authTokens.RefreshToken,
                ExpiresIn = authTokens.ExpiresIn,
                TokenExpiry = authTokens.AccessTokenExpiry,
                AccessTokenExpiresIn = authTokens.AccessTokenExpiresIn,
                RefreshTokenExpiresIn = authTokens.RefreshTokenExpiresIn,
                AccessTokenExpiry = authTokens.AccessTokenExpiry,
                RefreshTokenExpiry = authTokens.RefreshTokenExpiry,
                UserId = user.UserId,
                Username = user.Username,
                OrganisationId = organisationIdOverride ?? user.OrganisationId,
                AttendanceEnabled = locationTracking.AttendanceEnabled,
                EnableLocationTracking = effectiveEnableLocationTracking,
                EnableEmployeeLevelLocationTracking = effectiveEnableEmployeeLevelLocationTracking,
                EmployeeLocationTrackingEnabled = effectiveEmployeeLocationTrackingEnabled,
                IsGeoLocationEnabled = tenantConfig?.IsGeoLocationEnabled ?? false,
                IsGeoFencingEnabled = isGeoFencingEnabled,
                Latitude = isGeoFencingEnabled ? tenantConfig?.Latitude : null,
                Longitude = isGeoFencingEnabled ? tenantConfig?.Longitude : null,
                Radius = isGeoFencingEnabled ? tenantConfig?.Radius : null,
                LocationAddress = isGeoFencingEnabled ? tenantConfig?.LocationAddress : null,
                IsActive = tenantConfig?.IsActive ?? false,
                ModuleAccess = moduleAccess,
                WorkRoles = workRoles != null
                    ? workRoles.ToList()
                    : WorkRoleHelper.BuildLoginWorkRoles(null)
            };
        }

        /// <summary>
        /// Validates a password against a user's stored hash and salt
        /// Uses the same validation process as login to ensure consistency
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <param name="user">User object containing PasswordHash and PasswordSalt</param>
        /// <returns>True if password is valid, false otherwise</returns>
        private bool ValidateUserPassword(string password, User user)
        {
            // Same validation logic as login (lines 71-78)
            // Validate password against PasswordHash and PasswordSalt
            if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt))
            {
                return false;
            }

            // Use the same verification method as login
            return PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        }

        /// <summary>
        /// Masks contact information for privacy (e.g., "****1234" for mobile)
        /// </summary>
        private static string MaskContact(string contact)
        {
            if (string.IsNullOrEmpty(contact))
                return "****";

            if (contact.Contains('@'))
            {
                // Email masking: show first 2 chars and domain
                var parts = contact.Split('@');
                if (parts.Length == 2 && parts[0].Length > 2)
                {
                    return parts[0][..2] + new string('*', parts[0].Length - 2) + "@" + parts[1];
                }
            }

            // Mobile/Username masking: show last 4 chars
            if (contact.Length <= 4)
                return new string('*', contact.Length);

            return new string('*', contact.Length - 4) + contact[^4..];
        }

        /// <summary>
        /// Masks mobile number for privacy (e.g., "****3210")
        /// </summary>
        private static string MaskMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber) || mobileNumber.Length <= 4)
                return "****";

            return new string('*', mobileNumber.Length - 4) + mobileNumber[^4..];
        }

        /// <summary>
        /// Normalizes mobile number by removing non-digit characters
        /// </summary>
        private static string NormalizeMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return string.Empty;

            // Remove all non-digit characters
            return new string(mobileNumber.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Validates mobile number format (exactly 10 digits)
        /// </summary>
        private static bool IsValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return false;

            // Must be exactly 10 digits
            return Regex.IsMatch(mobileNumber, @"^\d{10}$");
        }
    }
}
