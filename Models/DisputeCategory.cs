namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the DisputeCategory table in the database
    /// </summary>
    public class DisputeCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

