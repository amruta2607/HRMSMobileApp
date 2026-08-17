namespace MobileWebApi.Services
{
    /// <summary>
    /// Represents a validation failure while processing a scanned asset QR code.
    /// </summary>
    public class ScannerValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerValidationException"/> class.
        /// </summary>
        /// <param name="message">Validation message.</param>
        public ScannerValidationException(string message) : base(message)
        {
        }
    }
}
