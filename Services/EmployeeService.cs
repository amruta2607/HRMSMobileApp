using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;

namespace MobileWebApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Resolves EmployeeId from UserId by joining Users and Employee tables
        /// </summary>
        private async Task<int?> ResolveEmployeeIdFromUserIdAsync(int userId)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (employee == null)
                {
                    _logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, userId);
                    return null;
                }
                return employee.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.EmployeeResolution.ErrorResolvingEmployeeIdFromUserId, userId);
                return null;
            }
        }

        public async Task<PersonalDetailServiceResponse> GetEmployeeByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.RetrievingEmployeeById, id);
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(id);
                if (queryResult == null)
                {
                    _logger.LogWarning(LogMessages.Employee.EmployeeNotFound, id);
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                // Map query result to DTO
                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeRetrievedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId // For access control
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Employee.ErrorRetrievingEmployee, id);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorRetrievingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> GetLoggedInEmployeeAsync(int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.RetrievingEmployeeByUserId, userId);
                // First get the employee to get the Id
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (employee == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeProfileNotFound,
                        Data = null
                    };
                }

                // Now get the personal details DTO using the employee Id
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(employee.Id);
                if (queryResult == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeProfileNotFound,
                        Data = null
                    };
                }

                // Map query result to DTO
                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeRetrievedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.GetLoggedInEmployee, nameof(GetLoggedInEmployeeAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorRetrievingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailListResponse> GetEmployeesByBranchAsync(int branchId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.RetrievingEmployeesByBranch, branchId);
                var employees = await _employeeRepository.GetEmployeesByBranchAsync(branchId);
                var employeeList = employees.ToList();

                return new PersonalDetailListResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeesRetrievedSuccessfully,
                    Data = employeeList,
                    TotalRecords = employeeList.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.GetEmployeesByBranch, nameof(GetEmployeesByBranchAsync), ex);
                return new PersonalDetailListResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorRetrievingEmployee,
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        public async Task<PersonalDetailListResponse> GetEmployeesByBranchExceptUserAsync(int branchId, int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.RetrievingEmployeesByBranch, branchId);
                var employees = await _employeeRepository.GetEmployeesByBranchExceptUserAsync(branchId, userId);
                var employeeList = employees.ToList();

                return new PersonalDetailListResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeesRetrievedSuccessfully,
                    Data = employeeList,
                    TotalRecords = employeeList.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.GetEmployeesByBranchExceptUser, nameof(GetEmployeesByBranchExceptUserAsync), ex);
                return new PersonalDetailListResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorRetrievingEmployee,
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> AddEmployeeAsync(PersonalDetailAddRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.AddingEmployee);
                
                if (request == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.RequestCannotBeNull,
                        Data = null
                    };
                }

                // Split addresses
                var current = ParseAddress(request.current_address);
                var permanent = ParseAddress(request.permanent_address);
                var fullName = ParseFullName(request.name);

                // Resolve organisation ID (can be string name or ID)
                int? organisationId = null;
                if (!string.IsNullOrWhiteSpace(request.organisations))
                {
                    if (int.TryParse(request.organisations, out int orgId))
                    {
                        organisationId = orgId;
                    }
                    else
                    {
                        organisationId = await _employeeRepository.GetOrganisationIdByNameAsync(request.organisations);
                    }
                }

                // Use provided IDs directly (or resolve from address strings if IDs not provided)
                var designationId = request.designationId;
                var branchId = request.branchId;
                var departmentId = request.departmentId;
                var genderId = request.genderId;
                var bloodgroupId = request.bloodGroupId;
                var maritalStatusId = request.maritalStatusId;
                
                // For state and country, use provided IDs or try to resolve from address strings
                var stateId = request.stateId;
                if (stateId == null && !string.IsNullOrWhiteSpace(current.State))
                {
                    stateId = await _employeeRepository.GetStateIdByNameAsync(current.State);
                }
                
                var countryId = request.countryId;
                if (countryId == null && !string.IsNullOrWhiteSpace(current.Country))
                {
                    countryId = await _employeeRepository.GetCountryIdByNameAsync(current.Country);
                }
                
                var permanantCountryId = request.permanentCountryId;
                if (permanantCountryId == null && !string.IsNullOrWhiteSpace(permanent.Country))
                {
                    permanantCountryId = await _employeeRepository.GetCountryIdByNameAsync(permanent.Country);
                }

                var employee = new Employee
                {
                    Name = request.name,
                    Email = request.official_email_id,
                    PersonalEmail = request.personal_email_id,
                    Phone = request.mobile_number,
                    OrganisationId = organisationId ?? 0,
                    GenderId = genderId ?? 0,
                    BloodGroup = bloodgroupId ?? 0,
                    MaritalStatus = maritalStatusId ?? 0,
                    DateOfJoining = request.date_of_joining,
                    DateOfBirth = request.date_of_birth,
                    DesignationId = designationId ?? 0,
                    BranchId = branchId ?? 0,
                    DepartmentId = departmentId ?? 0,
                    Street = current.Street,
                    City = current.City,
                    StateId = stateId ?? 0,
                    CountryId = countryId ?? 0,
                    ZipCode = current.Zip,
                    FirstName = fullName.firstName,
                    LastName = fullName.lastName,
                    SystemUserId = request.userId,
                    PermanentStreet = permanent.Street,
                    PermanentCity = permanent.City,
                    PermanentState = permanent.State,
                    PermanentCountryId = permanantCountryId ?? 0,
                    PermanentZipCode = permanent.Zip,
                    SalarySlab = 0,
                    BasicSalary = 25000,
                    IsPerDayWagesEmployee = false,
                    LeaveQuota = 0,
                    LeaveTaken = 0,
                    BankAccountForPayroll = "764523902345",
                    IFSCCode = "HDFC452",
                    BankNameForPayroll = "HDFC",
                    BankBranchName = "Kolhapur",
                    IsEmployeeActive = true,
                    IsPayrollOnHold = false,
                    EmployeeNumber = await _employeeRepository.GenerateEmployeeNumberAsync(organisationId ?? 0)
                };

                var newEmployee = await _employeeRepository.AddEmployeeAsync(employee);

                // Get the personal details DTO for the newly created employee
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(newEmployee.Id);
                if (queryResult == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeAddedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.AddEmployee, nameof(AddEmployeeAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorAddingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> UpdateEmployeeAsync(PersonalDetailUpdateRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.UpdatingEmployee, request.EmployeeId);
                
                if (request == null || request.EmployeeId <= 0)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.InvalidRequestOrEmployeeId,
                        Data = null
                    };
                }

                // Check if employee exists
                var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId);
                if (existingEmployee == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                // Parse addresses
                var current = ParseAddress(request.current_address);
                var permanent = ParseAddress(request.permanent_address);
                var fullName = ParseFullName(request.name);

                // Resolve lookup IDs (only if provided)
                int? designationId = string.IsNullOrEmpty(request.job_title) ? null :
                    await _employeeRepository.GetDesignationIdByNameAsync(request.job_title);
                int? branchId = string.IsNullOrEmpty(request.branch) ? null :
                    await _employeeRepository.GetBranchIdByNameAsync(request.branch);
                int? departmentId = string.IsNullOrEmpty(request.department) ? null :
                    await _employeeRepository.GetDepartmentIdByNameAsync(request.department);
                int? genderId = string.IsNullOrEmpty(request.gender) ? null :
                    await _employeeRepository.GetGenderIdByNameAsync(request.gender);
                int? bloodgroupId = string.IsNullOrEmpty(request.blood_group) ? null :
                    await _employeeRepository.GetBloodgroupIdByNameAsync(request.blood_group);
                int? maritalStatusId = string.IsNullOrEmpty(request.marital_status) ? null :
                    await _employeeRepository.GetMaritalStatusIdByNameAsync(request.marital_status);
                int? stateId = string.IsNullOrEmpty(current.State) ? null :
                    await _employeeRepository.GetStateIdByNameAsync(current.State);
                int? countryId = string.IsNullOrEmpty(current.Country) ? null :
                    await _employeeRepository.GetCountryIdByNameAsync(current.Country);
                int? permanentCountryId = string.IsNullOrEmpty(permanent.Country) ? null :
                    await _employeeRepository.GetCountryIdByNameAsync(permanent.Country);

                // Build employee object with updated values
                var employee = new Employee
                {
                    Id = request.EmployeeId,
                    Name = request.name,
                    Email = request.official_email_id,
                    PersonalEmail = request.personal_email_id,
                    Phone = request.mobile_number,
                    GenderId = genderId ?? existingEmployee.GenderId,
                    BloodGroup = bloodgroupId ?? existingEmployee.BloodGroup,
                    MaritalStatus = maritalStatusId ?? existingEmployee.MaritalStatus,
                    DateOfJoining = request.date_of_joining ?? existingEmployee.DateOfJoining,
                    DateOfBirth = request.date_of_birth ?? existingEmployee.DateOfBirth,
                    DesignationId = designationId ?? existingEmployee.DesignationId,
                    BranchId = branchId ?? existingEmployee.BranchId,
                    DepartmentId = departmentId ?? existingEmployee.DepartmentId,
                    Street = current.Street ?? existingEmployee.Street,
                    City = current.City ?? existingEmployee.City,
                    StateId = stateId ?? existingEmployee.StateId,
                    CountryId = countryId ?? existingEmployee.CountryId,
                    ZipCode = current.Zip ?? existingEmployee.ZipCode,
                    FirstName = fullName.firstName ?? existingEmployee.FirstName,
                    LastName = fullName.lastName ?? existingEmployee.LastName,
                    PermanentStreet = permanent.Street ?? existingEmployee.PermanentStreet,
                    PermanentCity = permanent.City ?? existingEmployee.PermanentCity,
                    PermanentState = permanent.State ?? existingEmployee.PermanentState,
                    PermanentCountryId = permanentCountryId ?? existingEmployee.PermanentCountryId,
                    PermanentZipCode = permanent.Zip ?? existingEmployee.PermanentZipCode,
                    IsEmployeeActive = request.is_active ?? existingEmployee.IsEmployeeActive
                };

                var success = await _employeeRepository.UpdateEmployeeAsync(employee);

                if (!success)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.FailedToUpdateEmployee,
                        Data = null
                    };
                }

                // Get the personal details DTO for the updated employee
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(request.EmployeeId);
                if (queryResult == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeUpdatedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.UpdateEmployee, nameof(UpdateEmployeeAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorUpdatingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> UpdateEmployeePhoneAndPictureAsync(PersonalDetailPhonePictureUpdateRequestInternal request)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId);
                if (!employeeId.HasValue)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = "Employee not found for the given user",
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Employee.UpdatingEmployee, employeeId.Value);
                
                if (request == null || request.UserId <= 0)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = "UserId is required.",
                        Data = null
                    };
                }

                // Check if employee exists
                var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeId.Value);
                if (existingEmployee == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                // Validate that at least one field is provided
                if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Picture))
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = "At least one field (Phone or Picture) must be provided for update.",
                        Data = null
                    };
                }

                // Update only phone and picture
                var success = await _employeeRepository.UpdateEmployeePhoneAndPictureAsync(
                    employeeId.Value, 
                    request.Phone, 
                    request.Picture);

                if (!success)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.FailedToUpdateEmployee,
                        Data = null
                    };
                }

                // Get the personal details DTO for the updated employee
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(employeeId.Value);
                if (queryResult == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeUpdatedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.UpdateEmployeePhoneAndPicture, nameof(UpdateEmployeePhoneAndPictureAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorUpdatingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> DeleteEmployeeAsync(int userId)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = "Employee not found for the given user",
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Employee.DeletingEmployee, employeeId.Value);
                
                var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeId.Value);
                if (existingEmployee == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var success = await _employeeRepository.DeleteEmployeeAsync(employeeId.Value);

                if (!success)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.FailedToDeleteEmployee,
                        Data = null
                    };
                }

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeDeletedSuccessfully,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.DeleteEmployee, nameof(DeleteEmployeeAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorDeletingEmployee,
                    Data = null
                };
            }
        }

        public async Task<PersonalDetailServiceResponse> DeactivateEmployeeAsync(int id)
        {
            try
            {
                _logger.LogInformation(LogMessages.Employee.DeactivatingEmployee, id);
                
                var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(id);
                if (existingEmployee == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var success = await _employeeRepository.DeactivateEmployeeAsync(id);

                if (!success)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.FailedToDeactivateEmployee,
                        Data = null
                    };
                }

                // Get the personal details DTO for the deactivated employee
                var queryResult = await _employeeRepository.GetEmployeePersonalDetailsByIdAsync(id);
                if (queryResult == null)
                {
                    return new PersonalDetailServiceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var dto = MapToPersonalDetailResponseDto(queryResult);

                return new PersonalDetailServiceResponse
                {
                    Success = true,
                    Message = EmployeeMessages.EmployeeDeactivatedSuccessfully,
                    Data = dto,
                    SystemUserId = queryResult.SystemUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Employee.DeactivateEmployee, nameof(DeactivateEmployeeAsync), ex);
                return new PersonalDetailServiceResponse
                {
                    Success = false,
                    Message = EmployeeMessages.ErrorDeactivatingEmployee,
                    Data = null
                };
            }
        }

        #region Helper Methods

        /// <summary>
        /// Maps EmployeePersonalDetailsQueryResult to PersonalDetailResponseDto
        /// </summary>
        private PersonalDetailResponseDto MapToPersonalDetailResponseDto(EmployeePersonalDetailsQueryResult queryResult)
        {
            // Build full name: FirstName + MiddleName (if not null) + LastName, trimmed
            var nameParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(queryResult.FirstName))
                nameParts.Add(queryResult.FirstName.Trim());
            if (!string.IsNullOrWhiteSpace(queryResult.MiddleName))
                nameParts.Add(queryResult.MiddleName.Trim());
            if (!string.IsNullOrWhiteSpace(queryResult.LastName))
                nameParts.Add(queryResult.LastName.Trim());
            var fullName = string.Join(" ", nameParts).Trim();
            // Trim extra spaces (replace multiple spaces with single space)
            while (fullName.Contains("  "))
                fullName = fullName.Replace("  ", " ");

            // Build supervisor name (same logic)
            string? reportingManager = null;
            if (queryResult.SupervisorId.HasValue && queryResult.SupervisorId.Value > 0)
            {
                var supervisorNameParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(queryResult.SupervisorFirstName))
                    supervisorNameParts.Add(queryResult.SupervisorFirstName.Trim());
                if (!string.IsNullOrWhiteSpace(queryResult.SupervisorMiddleName))
                    supervisorNameParts.Add(queryResult.SupervisorMiddleName.Trim());
                if (!string.IsNullOrWhiteSpace(queryResult.SupervisorLastName))
                    supervisorNameParts.Add(queryResult.SupervisorLastName.Trim());
                reportingManager = string.Join(" ", supervisorNameParts).Trim();
                // Trim extra spaces
                while (!string.IsNullOrEmpty(reportingManager) && reportingManager.Contains("  "))
                    reportingManager = reportingManager.Replace("  ", " ");
                if (string.IsNullOrWhiteSpace(reportingManager))
                    reportingManager = null;
            }

            // Build address object
            AddressDto? address = null;
            if (!string.IsNullOrWhiteSpace(queryResult.Street) ||
                !string.IsNullOrWhiteSpace(queryResult.City) ||
                !string.IsNullOrWhiteSpace(queryResult.State) ||
                !string.IsNullOrWhiteSpace(queryResult.ZipCode) ||
                !string.IsNullOrWhiteSpace(queryResult.Country))
            {
                address = new AddressDto
                {
                    Street = queryResult.Street,
                    City = queryResult.City,
                    State = queryResult.State,
                    ZipCode = queryResult.ZipCode,
                    Country = queryResult.Country
                };
            }

            return new PersonalDetailResponseDto
            {
                EmpId = queryResult.EmpId,
                Name = fullName,
                Picture = queryResult.Picture,
                Phone = queryResult.Phone,
                Email = queryResult.Email,
                Designation = queryResult.Designation,
                Address = address,
                ReportingManager = reportingManager
            };
        }

        private (string? Street, string? City, string? State, string? Country, string? Zip) ParseAddress(string? fullAddress)
        {
            if (string.IsNullOrWhiteSpace(fullAddress))
                return (null, null, null, null, null);

            var parts = fullAddress.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim())
                                   .ToArray();

            return (
                Street: parts.Length > 0 ? parts[0] : null,
                City: parts.Length > 1 ? parts[1] : null,
                State: parts.Length > 2 ? parts[2] : null,
                Country: parts.Length > 3 ? parts[3] : null,
                Zip: parts.Length > 4 ? parts[4] : null
            );
        }

        private (string? firstName, string? lastName) ParseFullName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (null, null);

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToArray();

            return (
                firstName: parts.Length > 0 ? parts[0] : null,
                lastName: parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : null
            );
        }

        private async Task<int?> ResolveIdAsync(string? input, Func<string, Task<int?>> lookupFunc, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var id = await lookupFunc(input);
            return id;
        }

        #endregion
    }
}
