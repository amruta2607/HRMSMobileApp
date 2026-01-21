using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MobileWebApi.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly ILogger<ImageUploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".png" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png" };

        public ImageUploadService(
            ILogger<ImageUploadService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        #region Validation

        public (bool IsValid, string ErrorMessage) ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Image file is required.");

            if (file.Length > MaxFileSize)
                return (false, "Image size must be less than 2 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return (false, "Only .jpg and .png images are allowed.");

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "Invalid image content type.");

            return (true, string.Empty);
        }

        #endregion

        #region Save Employee Image

        public async Task<string> SaveEmployeeImageAsync(
            IFormFile file,
            string webRootPath,
            int employeeId)
        {
            var validation = ValidateImage(file);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage);

            var uploadRoot = GetUploadBasePath(webRootPath);

            var folderValue = employeeId / 1000;
            var folderName = folderValue.ToString("D5");

            var uploadDir = Path.Combine(uploadRoot, "Image", "Employee", folderName);
            Directory.CreateDirectory(uploadDir);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var random = GenerateRandomString(15);
            var fileName = $"{employeeId:D8}_{random}{extension}";

            var fullPath = Path.Combine(uploadDir, fileName);

            // Save original image
            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            // 🔥 CREATE THUMBNAIL (THIS FIXES UI ISSUE)
            var thumbPath = Path.Combine(
                uploadDir,
                $"{employeeId:D8}_{random}_t.jpg");

            CreateThumbnail(fullPath, thumbPath);

            _logger.LogInformation("Employee image uploaded: {Path}", fullPath);

            return $"Image/Employee/{folderName}/{fileName}";
        }

        #endregion

        #region Thumbnail (CRITICAL FOR SERENITY UI)

        private void CreateThumbnail(string sourcePath, string thumbPath)
        {
            using var input = File.OpenRead(sourcePath);
            using var image = SixLabors.ImageSharp.Image.Load(input);

            image.Mutate(x =>
            {
                x.AutoOrient();
                x.Resize(new ResizeOptions
                {
                    Size = new Size(128, 128),
                    Mode = ResizeMode.Max
                });
            });

            using var output = File.OpenWrite(thumbPath);
            image.Save(
                output,
                new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                {
                    Quality = 80
                });
        }



        #endregion

        #region Helpers

        private string GetUploadBasePath(string webRootPath)
        {
            var path = _configuration["UploadSettings:Path"];

            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogWarning("UploadSettings:Path not configured. Using wwwroot.");
                return webRootPath;
            }

            return Path.GetFullPath(path);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
        }

        #endregion
    }
}
