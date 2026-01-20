namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for submitting a dispute
    /// </summary>
    public class DisputeSubmitRequest
    {
        public int UserId { get; set; }
        public int DisputeCategoryId { get; set; }
        public DateTime DisputeDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

