namespace MobileWebApi.Models
{
    /// <summary>
    /// Query result model for GetEmployeePersonalDetailsById query
    /// </summary>
    public class EmployeePersonalDetailsQueryResult
    {
        /// <summary>
        /// Employee Number (business identifier) - mapped from EmployeeNumber column
        /// </summary>
        public string EmpId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Picture { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorFirstName { get; set; }
        public string? SupervisorMiddleName { get; set; }
        public string? SupervisorLastName { get; set; }
        public int SystemUserId { get; set; } // For access control, not exposed in DTO
    }
}

