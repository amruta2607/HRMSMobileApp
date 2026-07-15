namespace MobileWebApi.Services
{
    /// <summary>
    /// Represents a validation failure while creating or updating an asset.
    /// </summary>
    public class AssetValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValidationException"/> class.
        /// </summary>
        /// <param name="message">Validation message.</param>
        public AssetValidationException(string message) : base(message)
        {
        }
    }
}
