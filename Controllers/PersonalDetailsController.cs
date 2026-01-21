using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using Microsoft.AspNetCore.Hosting;

namespace MobileWebApi.Controllers
{
    [Route("api/personal-details")]
    [ApiController]
    [Authorize]
    public class PersonalDetailsController : TenantBaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserRepository _userRepository;
        private readonly IImageUploadService _imageUploadService;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmployeeRepository _employeeRepository;

        public PersonalDetailsController(
            IEmployeeService employeeService, 
            IUserRepository userRepository,
            IImageUploadService imageUploadService,
            IEmployeeRepository employeeRepository,
            ITenantContext tenantContext,
            IWebHostEnvironment environment,
            ILogger<PersonalDetailsController> logger)
            : base(tenantContext, logger)
        {
            _employeeService = employeeService;
            _userRepository = userRepository;
            _imageUploadService = imageUploadService;
            _employeeRepository = employeeRepository;
            _environment = environment;
        }

        /// <summary>
        /// Get personal details by employee ID
        /// Note: Regular users can only access their own details. HR/TenantAdmin can access all.
        /// </summary>
        [HttpGet("Personal-Details-by-ID/{id}")]
        public async Task<IActionResult> GetPersonalDetailsById(int id)
        {
            if (id <= 0)
            {
                Logger.LogWarning(EmployeeMessages.InvalidEmployeeId);
                return BadRequest(new { Message = EmployeeMessages.InvalidEmployeeId });
            }

            Logger.LogInformation(LogMessages.Employee.RetrievingEmployeeById, id);
            var result = await _employeeService.GetEmployeeByIdAsync(id);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.Employee.EmployeeNotFound, id);
                return NotFound(result);
            }

            // Validate user access - regular users can only see their own data
            if (result.SystemUserId > 0)
            {
                if (!CanAccessUser(result.SystemUserId))
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedAccessToPersonalDetails, 
                        CurrentUserId, id, result.SystemUserId);
                    return UserAccessDenied();
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Get personal details by user ID
        /// Note: Regular users can only access their own details. HR/TenantAdmin can access all.
        /// </summary>
        [HttpGet("Personal-Details-by-user/{userId}")]
        public async Task<IActionResult> GetPersonalDetailsByUser(int userId)
        {
            if (userId <= 0)
            {
                Logger.LogWarning(EmployeeMessages.InvalidUserId);
                return BadRequest(new { Message = EmployeeMessages.InvalidUserId });
            }

            // Validate user access - regular users can only access their own data
            try
            {
                var validatedUserId = GetValidatedUserId(userId);
                userId = validatedUserId;
            }
            catch (Services.TenantAccessException)
            {
                return UserAccessDenied();
            }

            // Get employee by userId to fetch employee number
            Logger.LogInformation(LogMessages.Employee.RetrievingEmployeeByUserId, userId);
            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
            
            if (employee == null)
            {
                Logger.LogWarning(LogMessages.PersonalDetails.EmployeeNotFoundForUserId, userId);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Employee not found for user ID: {userId}" 
                });
            }

            // Get employee number and use it to get the employee ID (empId)
            if (string.IsNullOrWhiteSpace(employee.EmployeeNumber))
            {
                Logger.LogWarning(LogMessages.PersonalDetails.EmployeeDoesNotHaveEmployeeNumber, employee.Id);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Employee does not have an employee number" 
                });
            }

            Logger.LogInformation(LogMessages.PersonalDetails.FoundEmployeeNumberForUserId, employee.EmployeeNumber, userId);
            var employeeByNumber = await _employeeRepository.GetEmployeeByEmployeeNumberAsync(employee.EmployeeNumber);
            
            if (employeeByNumber == null)
            {
                Logger.LogWarning(LogMessages.PersonalDetails.EmployeeNotFoundWithEmployeeNumber, employee.EmployeeNumber);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Employee not found with employee number: {employee.EmployeeNumber}" 
                });
            }

            int employeeId = employeeByNumber.Id;
            Logger.LogInformation(LogMessages.PersonalDetails.UsingEmployeeIdFromEmployeeNumber, employeeId, employee.EmployeeNumber);
            
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);
            
            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.Employee.EmployeeNotFound, employeeId);
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all personal details by branch
        /// </summary>
        [HttpGet("Personal-Details-by-branch/{branchId}")]
        public async Task<IActionResult> GetPersonalDetailsByBranch(int branchId)
        {
            if (branchId <= 0)
            {
                Logger.LogWarning(EmployeeMessages.InvalidBranchId);
                return BadRequest(new { Message = EmployeeMessages.InvalidBranchId });
            }

            // Get logged-in user ID from JWT token
            int loggedUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            Logger.LogInformation(LogMessages.Employee.RetrievingEmployeesByBranch, branchId);

            // Get logged-in user details
            var loggedUser = await _userRepository.GetUserByIdAsync(loggedUserId);
            if (loggedUser == null)
            {
                Logger.LogWarning(UserMessages.UserNotFound);
                return Unauthorized(new { Message = UserMessages.UserNotFound });
            }

            // HR or Tenant Admin -> can see all employees
            if (loggedUser.IsHrUser || loggedUser.IsTenantAdmin)
            {
                var allEmployees = await _employeeService.GetEmployeesByBranchAsync(branchId);
                return Ok(allEmployees);
            }
            else
            {
                // Normal employee -> can see other employees except themselves
                var employees = await _employeeService.GetEmployeesByBranchExceptUserAsync(branchId, loggedUser.UserId);
                return Ok(employees);
            }
        }

        /// <summary>
        /// Add new personal details (create employee)
        /// Note: Employee will be created in user's organisation only.
        /// </summary>
        [HttpPost("Add-Personal-Details")]
        public async Task<IActionResult> AddPersonalDetails([FromBody] PersonalDetailAddRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(EmployeeMessages.RequestBodyNull);
                return BadRequest(new { Message = EmployeeMessages.RequestBodyNull });
            }

            if (string.IsNullOrWhiteSpace(request.name) || string.IsNullOrWhiteSpace(request.email_id))
            {
                Logger.LogWarning(EmployeeMessages.NameAndEmailRequired);
                return BadRequest(new { Message = EmployeeMessages.NameAndEmailRequired });
            }

            // Enforce tenant isolation - set organisation from user's token
            var userOrgId = CurrentOrganisationId;
            if (string.IsNullOrWhiteSpace(request.organisations))
            {
                request.organisations = userOrgId.ToString();
            }
            else
            {
                // Validate that provided organisation matches user's org
                if (int.TryParse(request.organisations, out int requestedOrgId))
                {
                    TenantContext.ValidateTenantAccess(requestedOrgId);
                }
                else
                {
                    request.organisations = userOrgId.ToString();
                }
            }

            Logger.LogInformation(LogMessages.Employee.AddingEmployee);
            var result = await _employeeService.AddEmployeeAsync(request);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.Employee.ErrorAddingEmployee);
                return BadRequest(result);
            }

            // Get the employee ID from EmployeeNumber for CreatedAtAction route parameter
            int employeeId = 0;
            if (result.Data != null && !string.IsNullOrWhiteSpace(result.Data.EmpId))
            {
                var employee = await _employeeRepository.GetEmployeeByEmployeeNumberAsync(result.Data.EmpId);
                if (employee != null)
                {
                    employeeId = employee.Id;
                }
            }

            return CreatedAtAction(nameof(GetPersonalDetailsById), new { id = employeeId }, result);
        }

        /// <summary>
        /// Update personal details - phone and picture only
        /// PUT: api/personal-details
        /// Accepts multipart/form-data with UserId, Phone (optional), and Picture file (optional)
        /// If Picture is provided, it will be validated, saved to wwwroot/Image/Employee/, and the relative path stored in database
        /// Note: Regular users can only update their own details. HR/TenantAdmin can update all.
        /// </summary>
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePersonalDetailsPhoneAndPicture([FromForm] PersonalDetailPhonePictureUpdateRequest request)
        {
            if (request == null || request.UserId <= 0)
            {
                Logger.LogWarning(LogMessages.PersonalDetails.InvalidRequestOrUserId);
                return BadRequest(new { Message = "UserId is required." });
            }

            // Validate that at least one field is provided
            if (string.IsNullOrWhiteSpace(request.Phone) && (request.Picture == null || request.Picture.Length == 0))
            {
                Logger.LogWarning(LogMessages.PersonalDetails.BothPhoneAndPictureEmpty);
                return BadRequest(new { Message = "At least one field (Phone or Picture) must be provided for update." });
            }

            // Resolve EmployeeId from UserId for access validation and picture saving
            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(request.UserId);
            if (employee == null)
            {
                Logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, request.UserId);
                return BadRequest(new { Message = "Employee not found for the given user" });
            }

            var employeeId = employee.Id;

            // Validate user access - regular users can only update their own data
            if (!HasElevatedAccess)
            {
                if (request.UserId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedUpdatePersonalDetails, 
                        CurrentUserId, employeeId);
                    return UserAccessDenied();
                }
            }

            string? savedPicturePath = null;

            // Handle picture upload if provided
            if (request.Picture != null && request.Picture.Length > 0)
            {
                try
                {
                    // Validate image
                    var validation = _imageUploadService.ValidateImage(request.Picture);
                    if (!validation.IsValid)
                    {
                        Logger.LogWarning(LogMessages.PersonalDetails.ImageValidationFailed, validation.ErrorMessage);
                        return BadRequest(new { Message = validation.ErrorMessage });
                    }

                    // Save image to shared upload folder and get relative path
                    // Physical path: {UploadSettings:RootPath}/Image/Employee/{folder}/
                    // Database path: Image/Employee/{folder}/{filename} (relative path)
                    // Folder is calculated as EmployeeId / 1000 (e.g., 420 → "00000", 1500 → "00001")
                    // Both Serenity UI and Web API use the same RootPath configuration pointing to a shared folder
                    var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                    savedPicturePath = await _imageUploadService.SaveEmployeeImageAsync(
                        request.Picture, 
                        webRootPath, 
                        employeeId);
                    Logger.LogInformation(LogMessages.PersonalDetails.PictureSavedSuccessfully, employeeId, request.UserId, savedPicturePath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, LogMessages.PersonalDetails.ErrorSavingEmployeePicture, request.UserId);
                    return StatusCode(500, new { Message = $"Error saving picture: {ex.Message}" });
                }
            }

            // Create service request with saved picture path (as string)
            var serviceRequest = new PersonalDetailPhonePictureUpdateRequestInternal
            {
                UserId = request.UserId,
                Phone = request.Phone,
                Picture = savedPicturePath // Pass the saved path as string
            };

            Logger.LogInformation(LogMessages.Employee.UpdatingEmployee, request.UserId);
            var result = await _employeeService.UpdateEmployeePhoneAndPictureAsync(serviceRequest);

            // Ensure the updated picture path is returned to the client immediately.
            // In some cases the service/repository can still return the previous value
            // (e.g. due to caching / projection delays). When we have just saved a new
            // image in this request, prefer that path so the UI can refresh without an
            // extra round‑trip or manual click.
            if (!string.IsNullOrWhiteSpace(savedPicturePath) &&
                result.Success &&
                result.Data != null)
            {
                result.Data.Picture = savedPicturePath.Replace("\\", "/");
            }

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.Employee.ErrorUpdatingEmployee, request.UserId);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update personal details
        /// Note: Regular users can only update their own details. HR/TenantAdmin can update all.
        /// </summary>
        //[HttpPut("Update-Personal-Details")]
        //public async Task<IActionResult> UpdatePersonalDetails([FromBody] PersonalDetailUpdateRequest request)
        //{
        //    if (request == null || request.EmployeeId <= 0)
        //    {
        //        Logger.LogWarning(EmployeeMessages.InvalidRequestOrEmployeeId);
        //        return BadRequest(new { Message = EmployeeMessages.InvalidRequestOrEmployeeId });
        //    }

        //    // Validate user access - regular users can only update their own data
        //    if (!HasElevatedAccess)
        //    {
        //        var existingEmployee = await _employeeService.GetEmployeeByIdAsync(request.EmployeeId);
        //        if (existingEmployee.Success && existingEmployee.Data != null)
        //        {
        //            if (existingEmployee.Data.SystemUserId != CurrentUserId)
        //            {
        //                Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedUpdatePersonalDetails, 
        //                    CurrentUserId, request.EmployeeId);
        //                return UserAccessDenied();
        //            }
        //        }
        //    }

        //    Logger.LogInformation(LogMessages.Employee.UpdatingEmployee, request.EmployeeId);
        //    var result = await _employeeService.UpdateEmployeeAsync(request);

        //    if (!result.Success)
        //    {
        //        Logger.LogWarning(LogMessages.Employee.ErrorUpdatingEmployee, request.EmployeeId);
        //        return BadRequest(result);
        //    }

        //    return Ok(result);
        //}

        /// <summary>
        /// Delete personal details (hard delete)
        /// Note: Regular users can only delete their own details. HR/TenantAdmin can delete all.
        /// </summary>
        [HttpDelete("Delete-Personal-Details/{userId}")]
        public async Task<IActionResult> DeletePersonalDetails(int userId)
        {
            if (userId <= 0)
            {
                Logger.LogWarning(LogMessages.PersonalDetails.InvalidUserId);
                return BadRequest(new { Message = "UserId is required." });
            }

            // Validate user access - regular users can only delete their own data
            if (!HasElevatedAccess)
            {
                if (userId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedDeletePersonalDetails, 
                        CurrentUserId, userId);
                    return UserAccessDenied();
                }
            }

            Logger.LogInformation(LogMessages.Employee.DeletingEmployee, userId);
            var result = await _employeeService.DeleteEmployeeAsync(userId);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.Employee.ErrorDeletingEmployee, userId);
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Upload employee picture
        /// DEPRECATED: Use PUT api/personal-details instead which now supports picture upload
        /// POST: api/personal-details/upload-picture
        /// Accepts multipart/form-data with employeeId and picture file (jpg, png, max 2MB)
        /// Saves image to wwwroot/Image/Employee/ with GUID-based filename
        /// Stores relative path in database (matches Serenity ImageUploadEditor behavior)
        /// Note: Regular users can only upload pictures for their own employee record. HR/TenantAdmin can upload for any employee.
        /// </summary>
        //[HttpPost("upload-picture")]
        //[Consumes("multipart/form-data")]
        //[ProducesResponseType(typeof(EmployeePictureUploadResponse), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> UploadEmployeePicture([FromForm] EmployeePictureUploadRequest request)
        //{
        //    if (request == null || request.EmployeeId <= 0)
        //    {
        //        Logger.LogWarning(EmployeeMessages.InvalidRequestOrEmployeeId);
        //        return BadRequest(new EmployeePictureUploadResponse
        //        {
        //            Success = false,
        //            Message = EmployeeMessages.InvalidRequestOrEmployeeId
        //        });
        //    }

        //    if (request.Picture == null || request.Picture.Length == 0)
        //    {
        //        Logger.LogWarning("Picture file is required");
        //        return BadRequest(new EmployeePictureUploadResponse
        //        {
        //            Success = false,
        //            Message = "Picture file is required."
        //        });
        //    }

        //    // Validate user access - regular users can only upload pictures for their own employee record
        //    if (!HasElevatedAccess)
        //    {
        //        var existingEmployee = await _employeeService.GetEmployeeByIdAsync(request.EmployeeId);
        //        if (existingEmployee.Success && existingEmployee.Data != null)
        //        {
        //            if (existingEmployee.SystemUserId != CurrentUserId)
        //            {
        //                Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedUpdatePersonalDetails,
        //                    CurrentUserId, request.EmployeeId);
        //                return UserAccessDenied();
        //            }
        //        }
        //    }

        //    try
        //    {
        //        // Validate image
        //        var validation = _imageUploadService.ValidateImage(request.Picture);
        //        if (!validation.IsValid)
        //        {
        //            Logger.LogWarning("Image validation failed: {Error}", validation.ErrorMessage);
        //            return BadRequest(new EmployeePictureUploadResponse
        //            {
        //                Success = false,
        //                Message = validation.ErrorMessage
        //            });
        //        }

        //        // Save image and get relative path
        //        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        //        var savedImagePath = await _imageUploadService.SaveEmployeeImageAsync(request.Picture, webRootPath);

        //        // Update database with relative path
        //        var updateRequest = new PersonalDetailPhonePictureUpdateRequestInternal
        //        {
        //            EmployeeId = request.EmployeeId,
        //            Phone = null, // Only updating picture
        //            Picture = savedImagePath
        //        };

        //        Logger.LogInformation("Uploading picture for employee {EmployeeId}", request.EmployeeId);
        //        var updateResult = await _employeeService.UpdateEmployeePhoneAndPictureAsync(updateRequest);

        //        if (!updateResult.Success)
        //        {
        //            Logger.LogWarning("Failed to update employee picture in database: {Message}", updateResult.Message);
        //            return StatusCode(500, new EmployeePictureUploadResponse
        //            {
        //                Success = false,
        //                Message = $"Image saved but failed to update database: {updateResult.Message}"
        //            });
        //        }

        //        Logger.LogInformation("Picture uploaded successfully for employee {EmployeeId}: {Path}", request.EmployeeId, savedImagePath);

        //        return Ok(new EmployeePictureUploadResponse
        //        {
        //            Success = true,
        //            Message = "Picture uploaded successfully",
        //            PicturePath = savedImagePath
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, "Error uploading employee picture for employee {EmployeeId}", request.EmployeeId);
        //        return StatusCode(500, new EmployeePictureUploadResponse
        //        {
        //            Success = false,
        //            Message = $"Error uploading picture: {ex.Message}"
        //        });
        //    }
        //}

        /// <summary>
        /// Deactivate employee (soft delete)
        /// Note: Regular users can only deactivate their own account. HR/TenantAdmin can deactivate all.
        /// </summary>
        //[HttpPatch("{id}/deactivate")]
        //public async Task<IActionResult> DeactivateEmployee(int id)
        //{
        //    if (id <= 0)
        //    {
        //        Logger.LogWarning(EmployeeMessages.InvalidEmployeeId);
        //        return BadRequest(new { Message = EmployeeMessages.InvalidEmployeeId });
        //    }

        //    // Validate user access - regular users can only deactivate their own account
        //    if (!HasElevatedAccess)
        //    {
        //        var existingEmployee = await _employeeService.GetEmployeeByIdAsync(id);
        //        if (existingEmployee.Success && existingEmployee.Data != null)
        //        {
        //            if (existingEmployee.Data.SystemUserId != CurrentUserId)
        //            {
        //                Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedDeactivateEmployee, 
        //                    CurrentUserId, id);
        //                return UserAccessDenied();
        //            }
        //        }
        //    }

        //    Logger.LogInformation(LogMessages.Employee.DeactivatingEmployee, id);
        //    var result = await _employeeService.DeactivateEmployeeAsync(id);

        //    if (!result.Success)
        //    {
        //        Logger.LogWarning(LogMessages.Employee.ErrorDeactivatingEmployee, id);
        //        return NotFound(result);
        //    }

        //    return Ok(result);
        //}

    }
}
