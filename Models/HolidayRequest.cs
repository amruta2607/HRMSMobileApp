namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for creating a holiday (OrdiNet compatible)
    /// </summary>
    public class HolidayCreateRequest
    {
        /// <summary>
        /// Holiday name
        /// </summary>
        public string holiday_name { get; set; } = string.Empty;
        
        /// <summary>
        /// Holiday date
        /// </summary>
        public DateTime date { get; set; }
        
        /// <summary>
        /// Optional description
        /// </summary>
        public string? description { get; set; }
    }

    /// <summary>
    /// Request model for updating a holiday
    /// </summary>
    public class HolidayUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request model for bulk holiday creation
    /// </summary>
    public class HolidayBulkCreateRequest
    {
        public List<HolidayCreateRequest> Holidays { get; set; } = new List<HolidayCreateRequest>();
    }

    /// <summary>
    /// Request model for updating holiday date via form data
    /// </summary>
    public class HolidayUpdateDateRequest
    {
        /// <summary>
        /// Holiday ID
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// New holiday date
        /// </summary>
        public DateTime date { get; set; }
    }
}

