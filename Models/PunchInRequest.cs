using Microsoft.AspNetCore.Mvc;

namespace MobileWebApi.Models
{
    public class PunchInRequest
    {
        public int userId { get; set; }
        public DateTime punch_in_time { get; set; }
        public DateTime attendance_date { get; set; }




        public double? longitude { get; set; }
        public double? latitude { get; set; }
		[FromForm(Name = "punchInImage")]
		public IFormFile? PunchInImage { get; set; }


	}
}
