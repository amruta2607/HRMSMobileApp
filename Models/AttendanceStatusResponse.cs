namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for attendance status API
    /// </summary>
    public class AttendanceStatusResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public AttendanceStatusData? Data { get; set; }
    }

    /// <summary>
    /// Attendance status data
    /// </summary>
    public class AttendanceStatusData
    {
        /// <summary>
        /// Whether attendance is marked for the employee on the date
        /// </summary>
        public bool isMarked { get; set; }

        /// <summary>
        /// Whether attendance is already marked (prevents duplicate punch-in)
        /// </summary>
        public bool isAlreadyMarked { get; set; }

        /// <summary>
        /// Attendance status: "Present", "Absent", or "Not Marked"
        /// </summary>
        public string? status { get; set; }

        /// <summary>
        /// Punch table primary key when a punch exists for the date
        /// </summary>
        public int? PunchId { get; set; }

        /// <summary>
        /// Punch-in time if available
        /// </summary>
        public DateTime? punchIn { get; set; }

        /// <summary>
        /// Punch-out time if available
        /// </summary>
        public DateTime? punchOut { get; set; }

        /// <summary>
        /// Duration in hours if punch-out is done
        /// </summary>
        public double? duration { get; set; }

        /// <summary>
        /// Attendance date
        /// </summary>
        public DateTime date { get; set; }

        public string? inSource { get; set; }
        public string? outSource { get; set; }
        public string? coordinateIn { get; set; }
        public string? coordinateOut { get; set; }
        public string? linkIn { get; set; }
        public string? linkOut { get; set; }
    }
}

