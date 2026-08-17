using Microsoft.AspNetCore.Mvc;

namespace MobileWebApi.Models
{
    public class PunchOutRequest
    {
        public int userId { get; set; }
        public DateTime punch_out_time { get; set; }
        public DateTime attendance_date { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        [FromForm(Name = "punchOutImage")]
        public IFormFile? PunchOutImage { get; set; }
        public string? punchOutReason { get; set; }
    }
}
