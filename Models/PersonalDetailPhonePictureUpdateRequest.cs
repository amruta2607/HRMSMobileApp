using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for updating only phone and picture in personal details
    /// </summary>
    public class PersonalDetailPhonePictureUpdateRequest
    {
        /// <summary>
        /// User ID (from Users table)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Phone number to update
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Picture file to upload (jpg, png - max 2MB)
        /// </summary>
        public IFormFile? Picture { get; set; }
    }
}

