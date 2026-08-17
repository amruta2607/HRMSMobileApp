namespace MobileWebApi.Models
{
    /// <summary>
    /// DTO for Personal Details API response - contains only required fields
    /// </summary>
    public class PersonalDetailResponseDto
    {
        /// <summary>
        /// Employee Number (business identifier) - mapped from Employee.EmployeeNumber
        /// </summary>
        public string EmpId { get; set; } = string.Empty;

        /// <summary>
        /// Full name: FirstName + MiddleName (if not null) + LastName, trimmed
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Employee picture path (relative path like "Image/Employee/xyz.jpg")
        /// </summary>
        public string? Picture { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Professional email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Designation name from Designation table (employees) or WorkRoleName (non-employee users)
        /// </summary>
        public string? Designation { get; set; }

        /// <summary>
        /// Department from Users.department (populated for non-employee users)
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Current address merged object
        /// </summary>
        public AddressDto? Address { get; set; }

        /// <summary>
        /// Reporting manager full name (formatted same as Name) or null if SupervisorId is null
        /// </summary>
        public string? ReportingManager { get; set; }
    }

    /// <summary>
    /// Address DTO for current address
    /// </summary>
    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
    }
}

