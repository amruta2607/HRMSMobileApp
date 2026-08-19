using System.Text.Json;
using MobileWebApi.Models;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Serializes and deserializes the AssetMaintenance.Attachment column, which stores a
    /// list of <see cref="FileAttachment"/> references as a JSON string in the existing HRMS format.
    /// </summary>
    public static class AttachmentJsonHelper
    {
        private static readonly JsonSerializerOptions DeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the supplied attachments to a JSON string.
        /// Returns <c>null</c> when the list is null, empty, or contains only blank entries
        /// so the column is stored as SQL NULL.
        /// </summary>
        public static string? Serialize(List<FileAttachment>? attachments)
        {
            var valid = attachments?
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Filename))
                .ToList();

            if (valid == null || valid.Count == 0)
                return null;

            return JsonSerializer.Serialize(valid);
        }

        /// <summary>
        /// Deserializes the stored JSON string into a list of attachments.
        /// Returns <c>null</c> when the value is empty, or when it is a legacy non-JSON value
        /// that cannot be represented as an attachment list (backward compatibility).
        /// </summary>
        public static List<FileAttachment>? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var list = JsonSerializer.Deserialize<List<FileAttachment>>(json, DeserializeOptions);
                return list == null || list.Count == 0 ? null : list;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
