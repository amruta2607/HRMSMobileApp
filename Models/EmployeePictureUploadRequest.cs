using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for uploading employee picture
    /// </summary>
    public class EmployeePictureUploadRequest
    {
        /// <summary>
        /// Employee ID
        /// </summary>
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Picture file (jpg, png - max 2MB)
        /// </summary>
        [Required]
        public IFormFile Picture { get; set; } = null!;
    }
}
