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
		Task<string> SaveEmployeeImageAsync(IFormFile file, int employeeId);

		/// <summary>
		/// Validates an attachment/document file (size and allowed extension).
		/// </summary>
		/// <param name="file">The attachment file to validate.</param>
		/// <returns>Tuple with success status and error message (if any).</returns>
		(bool IsValid, string ErrorMessage) ValidateAttachment(IFormFile file);

		/// <summary>
		/// Saves an asset document to the shared upload folder using the Serenity-compatible
		/// filename format and returns the relative path stored in the database.
		/// Format: AssetDocument/{tenantId/1000 padded 5 digits}/{tenantId padded 8 digits}_{random}{extension}
		/// Example: AssetDocument/00000/00000002_fxusfogpf6lhs.pdf
		/// </summary>
		/// <param name="file">The document file to save.</param>
		/// <param name="tenantId">The tenant (organisation) identifier used for folder/filename generation.</param>
		/// <returns>Relative path of the saved document (e.g., "AssetDocument/00000/00000002_xxxxx.pdf").</returns>
		Task<string> SaveAssetDocumentAsync(IFormFile file, int tenantId);
	}
}