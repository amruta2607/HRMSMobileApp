using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Service for handling image upload operations
    /// </summary>
    public class ImageUploadService : IImageUploadService
    {
        private readonly ILogger<ImageUploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB (as per requirements)
        private static readonly string[] AllowedExtensions = { ".jpg", ".png" }; // Only jpg and png as per requirements
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png" };

        public ImageUploadService(ILogger<ImageUploadService> logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        public (bool IsValid, string ErrorMessage) ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Image file is required.");
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                return (false, $"Image file size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return (false, $"Invalid file format. Only {string.Join(", ", AllowedExtensions)} files are allowed.");
            }

            // Validate content type
            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, $"Invalid content type. Only {string.Join(", ", AllowedContentTypes)} are allowed.");
            }

            return (true, string.Empty);
        }

        public async Task<string> SaveEmployeeImageAsync(IFormFile file, string webRootPath, int employeeId)
        {
            try
            {
                // Validate image first
                var validation = ValidateImage(file);
                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                // Get upload base path from configuration (shared folder outside both projects)
                // This ensures images are saved to the same location accessible by both Serenity UI and Web API
                var uploadBasePath = GetUploadBasePath(webRootPath);
                
                // Ensure the base upload directory exists
                if (!Directory.Exists(uploadBasePath))
                {
                    Directory.CreateDirectory(uploadBasePath);
                    _logger.LogInformation(LogMessages.ImageUpload.CreatedBaseUploadDirectory, uploadBasePath);
                }
                
                _logger.LogInformation(LogMessages.ImageUpload.UsingUploadBasePath, uploadBasePath);

                // Get file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Generate random string (15 characters, lowercase alphanumeric) - matching Serenity format
                // Example random string: pxgzarl2tg7ek (15 chars: a-z, 0-9)
                var randomString = GenerateRandomString(15);

                // Generate filename: {employeeId padded}_{random}.{extension}
                // Example: 00000420_pxgzarl2tg7ek.jpg
                var fileName = GenerateEmployeeImageFileName(employeeId, randomString, fileExtension);

                // Calculate folder name: EmployeeId / 1000 (matching Serenity's {1:00000} parameter)
                // This matches Serenity's ImageUploadEditor FilenameFormat = "Image/Employee/~"
                // Example: EmployeeId 420 → 420 / 1000 = 0 → "00000"
                //          EmployeeId 1500 → 1500 / 1000 = 1 → "00001"
                var folderValue = employeeId / 1000;
                var folderName = folderValue.ToString("D5");

                // Create directory: Image/Employee/00000/
                var uploadDirectory = Path.Combine(uploadBasePath, "Image", "Employee", folderName);
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                    _logger.LogInformation(LogMessages.ImageUpload.CreatedDirectory, uploadDirectory);
                }

                // Full path to save the file
                var filePath = Path.Combine(uploadDirectory, fileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation(LogMessages.ImageUpload.ImageSavedSuccessfully, filePath);

                // Return relative path in Serenity format: Image/Employee/{folder}/{employeeId padded}_{random}.{extension}
                // Example: Image/Employee/00000/00000420_pxgzarl2tg7ek.jpg
                var relativePath = Path.Combine("Image", "Employee", folderName, fileName).Replace("\\", "/");
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ImageUpload.ErrorSavingEmployeeImage, file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Gets the upload base path from configuration (shared folder outside both projects)
        /// Uses UploadSettings:RootPath which should be an absolute path to a shared folder
        /// Example: C:\SharedUploads\Indotalent
        /// This ensures both Serenity UI and Web API save to the same physical location
        /// </summary>
        /// <param name="webRootPath">Fallback path if configuration is not set (should not be used in production)</param>
        /// <returns>Absolute path to the shared upload directory</returns>
        private string GetUploadBasePath(string webRootPath)
        {
            var rootPath = _configuration["UploadSettings:Path"];
            
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                // Default to wwwroot if not configured (fallback for development)
                _logger.LogWarning(LogMessages.ImageUpload.UploadPathNotConfiguredUsingFallback);
                return webRootPath;
            }

            // Normalize the path (handles both forward and backslashes, trailing slashes, etc.)
            var normalizedPath = Path.GetFullPath(rootPath);
            
            _logger.LogInformation(LogMessages.ImageUpload.UsingSharedUploadRootPath, normalizedPath);
            
            return normalizedPath;
        }

        /// <summary>
        /// Generates employee image filename in Serenity-compatible format
        /// Format: {employeeId padded to 8 digits}_{random string}.{extension}
        /// Example: 00000420_pxgzarl2tg7ek.jpg
        /// Random string: 15 characters, lowercase alphanumeric (a-z, 0-9)
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <param name="randomString">Random alphanumeric string (15 characters, e.g., "pxgzarl2tg7ek")</param>
        /// <param name="fileExtension">File extension (e.g., ".jpg")</param>
        /// <returns>Filename in format: {employeeId padded}_{random}.{extension}</returns>
        private string GenerateEmployeeImageFileName(int employeeId, string randomString, string fileExtension)
        {
            // Format employee ID as 8-digit padded string (e.g., "00000420")
            var employeeIdPadded = employeeId.ToString("D8");
            
            // Generate filename: {employeeId padded}_{random}.{extension}
            return $"{employeeIdPadded}_{randomString}{fileExtension}";
        }

        /// <summary>
        /// Generates a random lowercase alphanumeric string (matching Serenity's ImageUploadEditor format)
        /// </summary>
        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

