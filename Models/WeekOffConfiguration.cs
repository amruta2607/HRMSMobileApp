namespace MobileWebApi.Models
{
    public class WeekOffConfiguration
    {
        public HashSet<int> CompleteWeekOffDays { get; set; } = new HashSet<int>();
        public List<PartialWeekOffDayItem> PartialWeekOffDays { get; set; } = new List<PartialWeekOffDayItem>();
    }
}
