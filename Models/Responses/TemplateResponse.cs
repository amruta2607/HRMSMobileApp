namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Represents an active template available for selection in the mobile application.
    /// </summary>
    public class TemplateResponse
    {
        /// <summary>
        /// Template identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name of the template.
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;
    }
}
