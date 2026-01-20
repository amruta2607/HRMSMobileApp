namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for employee picture upload
    /// </summary>
    public class EmployeePictureUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PicturePath { get; set; }
    }
}

