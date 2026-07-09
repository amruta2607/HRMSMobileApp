namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Represents a single asset hand over record for the mobile application.
    /// </summary>
    public class AssetHandOverDto
    {
        /// <summary>
        /// Hand over identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Hand over number.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Related asset identifier.
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// Related asset number.
        /// </summary>
        public string AssetNumber { get; set; } = string.Empty;

        /// <summary>
        /// Related asset name.
        /// </summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Date when the asset was handed over.
        /// </summary>
        public DateTime HandOverDate { get; set; }

        /// <summary>
        /// Employee identifier who handed over the asset.
        /// </summary>
        public int? HandOverById { get; set; }

        /// <summary>
        /// Employee name who handed over the asset.
        /// </summary>
        public string HandOverBy { get; set; } = string.Empty;

        /// <summary>
        /// Employee identifier who received the asset.
        /// </summary>
        public int? HandOverToId { get; set; }

        /// <summary>
        /// Employee name who received the asset.
        /// </summary>
        public string HandOverTo { get; set; } = string.Empty;

        /// <summary>
        /// Department name from the related asset.
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Branch name from the related asset.
        /// </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// Hand over remarks.
        /// </summary>
        public string Remarks { get; set; } = string.Empty;
    }
}
