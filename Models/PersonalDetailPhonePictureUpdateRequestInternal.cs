namespace MobileWebApi.Models
{
    /// <summary>
    /// Internal request model for service layer - uses string path for picture
    /// </summary>
    public class PersonalDetailPhonePictureUpdateRequestInternal
    {
        public int UserId { get; set; }
        public string? Phone { get; set; }
        public string? Picture { get; set; } // Picture path as string
    }
}

