namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents a single stored file reference following the existing HRMS attachment format.
    /// Files are uploaded through the existing storage mechanism; only the resulting
    /// storage path (<see cref="Filename"/>) and the user-facing name (<see cref="OriginalName"/>) are persisted.
    /// </summary>
    public class FileAttachment
    {
        /// <summary>
        /// Relative storage path of the uploaded file (e.g. "AssetDocument/00000/00000002_fxusfogpf6lhs.pdf").
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Original file name shown to the user (e.g. "Work Position_20260723_1728.pdf").
        /// </summary>
        public string OriginalName { get; set; } = string.Empty;
    }
}
