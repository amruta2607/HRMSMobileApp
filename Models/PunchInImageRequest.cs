using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Multipart form request for punch-in with optional image upload.
    /// </summary>
    public class PunchInImageRequest
    {
        public int empId { get; set; }
        public DateTime punchTime { get; set; }

        [FromForm(Name = "punchInImage")]
        public IFormFile? PunchInImage { get; set; }
    }
}
