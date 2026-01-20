namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Service for handling image upload operations
    /// </summary>
    public interface IImageUploadService
    {
        /// <summary>
        /// Validates image file format and size
        /// </summary>
        /// <param name="file">The image file to validate</param>
        /// <returns>Tuple with success status and error message (if any)</returns>
        (bool IsValid, string ErrorMessage) ValidateImage(IFormFile file);

        /// <summary>
        /// Saves image file to shared upload folder with Serenity-compatible filename format
        /// Physical path: {UploadSettings:RootPath}/Image/Employee/{folder}/
        /// Database path: Image/Employee/{folder}/{filename} (relative path stored in DB)
        /// Format matches Serenity's ImageUploadEditor FilenameFormat = "Image/Employee/~"
        /// Format: Image/Employee/{employeeId/1000 padded 5 digits}/{employeeId padded 8 digits}_{random}.{extension}
        /// Example: Image/Employee/00000/00000420_pxgzarl2tg7ek.jpg
        /// Random string: 15 characters, lowercase alphanumeric (a-z, 0-9)
        /// Note: Folder is calculated as EmployeeId / 1000 (not TenantId)
        /// The upload root path is configured via UploadSettings:RootPath in appsettings.json
        /// Both Serenity UI and Web API should use the same RootPath value pointing to a shared folder outside both projects
        /// </summary>
        /// <param name="file">The image file to save</param>
        /// <param name="webRootPath">The web root path (used as fallback if configuration is not set)</param>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Relative path of saved image (e.g., "Image/Employee/00000/00000420_xxxxx.jpg")</returns>
        Task<string> SaveEmployeeImageAsync(IFormFile file, string webRootPath, int employeeId);
    }
}

