using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class ImageUploadService : IImageUploadService
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<ImageUploadService> _logger;

	private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
	private static readonly string[] AllowedExtensions = { ".jpg", ".png" };
	private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png" };

	public ImageUploadService(IConfiguration configuration, ILogger<ImageUploadService> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public (bool IsValid, string ErrorMessage) ValidateImage(IFormFile file)
	{
		if (file == null || file.Length == 0)
			return (false, "Image file is required.");

		if (file.Length > MaxFileSize)
			return (false, "Image size must be less than 2 MB.");

		var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
		if (!AllowedExtensions.Contains(ext))
			return (false, "Only .jpg and .png images are allowed.");

		if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
			return (false, "Invalid image content type.");

		return (true, string.Empty);
	}

	public async Task<string> SaveEmployeeImageAsync(IFormFile file, int employeeId)
	{
		var validation = ValidateImage(file);
		if (!validation.IsValid)
			throw new ArgumentException(validation.ErrorMessage);

		var uploadRoot = Path.GetFullPath(_configuration["UploadSettings:Path"]);

		// Folder structure: Image/Employee/00000
		var folderName = (employeeId / 1000).ToString("D5");
		var uploadDir = Path.Combine(uploadRoot, "Image", "Employee", folderName);
		Directory.CreateDirectory(uploadDir);

		var random = GenerateRandomString(15);
		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		var fileName = $"{employeeId:D8}_{random}{extension}";

		var fullPath = Path.Combine(uploadDir, fileName);

		using (var fs = new FileStream(fullPath, FileMode.Create))
			await file.CopyToAsync(fs);

		// Create thumbnail
		var thumbPath = Path.Combine(uploadDir, $"{employeeId:D8}_{random}_t.jpg");
		CreateThumbnail(fullPath, thumbPath);

		_logger.LogInformation(LogMessages.ImageUpload.ImageSavedSuccessfully, fullPath);

		// Return relative path for DB
		return $"Image/Employee/{folderName}/{fileName}";
	}

	private void CreateThumbnail(string sourcePath, string thumbPath)
	{
		using var image = SixLabors.ImageSharp.Image.Load(sourcePath);
		image.Mutate(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
		{
			Size = new SixLabors.ImageSharp.Size(128, 128),
			Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max
		}));
		image.Save(thumbPath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 90 });
	}

	private string GenerateRandomString(int length)
	{
		const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
		var random = new Random();
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
}
