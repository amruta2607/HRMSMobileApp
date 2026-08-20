using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Multipart form request for punch-out with optional image upload.
    /// </summary>
    public class PunchOutImageRequest
    {
        public int empId { get; set; }
        public DateTime punchTime { get; set; }

        [FromForm(Name = "punchOutImage")]
        public IFormFile? PunchOutImage { get; set; }
    }
}
