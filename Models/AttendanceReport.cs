namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents an attendance report record from the Altroz database
    /// </summary>
    public class AttendanceReport
    {
        public int Id { get; set; }

        /// <summary>
        /// Punch table primary key (same as <see cref="Id"/> when the row comes from Punch).
        /// </summary>
        public int? PunchId { get; set; }

        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime CalendarDate { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? WorkingDuration { get; set; }
        public string? Month { get; set; }
        public string? Source { get; set; } // "punch" or "timesheet"
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
        
        public int? SystemUserId { get; set; }

        public string? InSource { get; set; }
        public string? OutSource { get; set; }
        public string? CoordinateIn { get; set; }
        public string? CoordinateOut { get; set; }
        public string? LinkIn { get; set; }
        public string? LinkOut { get; set; }
        public string? ImageUrl { get; set; }
    }
}

