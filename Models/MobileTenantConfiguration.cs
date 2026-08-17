namespace MobileWebApi.Models
{
    public class MobileTenantConfiguration
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public bool IsEnableMobile { get; set; }
        public bool IsAttendanceEnabled { get; set; }
        public bool IsLeaveEnabled { get; set; }
        public bool IsPayrollEnabled { get; set; }
        public bool EnableLocationTracking { get; set; }
        public bool EnableEmployeeLevelLocationTracking { get; set; }
        public int? LocationTrackingConfigurationId { get; set; }
        public DateTime InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
    }
}

