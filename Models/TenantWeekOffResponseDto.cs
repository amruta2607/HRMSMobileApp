namespace MobileWebApi.Models
{
    public class TenantWeekOffResponseDto
    {
        public int TenantId { get; set; }
        public int TenantConfigurationId { get; set; }
        public List<TenantWeekOffDayDto> WeekOffDays { get; set; } = new();
    }
}
