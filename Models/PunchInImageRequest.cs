using Microsoft.AspNetCore.Http;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Multipart form request for punch-in with optional image upload.
    /// </summary>
    public class PunchInImageRequest
    {
        public int empId { get; set; }
        public DateTime punchTime { get; set; }

        public IFormFile? image { get; set; }
    }
}

