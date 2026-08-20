using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// true = employee manually punched out; false = system auto punch-out. Defaults to true when omitted.
        /// </summary>
        public bool? Manual { get; set; }

        public string? PunchOutReason { get; set; }

        [FromForm(Name = "punchOutImage")]
        public IFormFile? PunchOutImage { get; set; }

        /// <summary>
        /// Alternate image field used by location-tracking punch flow.
        /// </summary>
        public IFormFile? image { get; set; }
    }
}
