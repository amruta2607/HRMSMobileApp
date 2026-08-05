namespace MobileWebApi.Models
{
    public class PunchOutRequest
    {
        public int userId { get; set; }
        public DateTime punch_out_time { get; set; }
        public DateTime attendance_date { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public IFormFile? image { get; set; }
    }
}
