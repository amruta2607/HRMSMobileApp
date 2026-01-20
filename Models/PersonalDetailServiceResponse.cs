using System.Text.Json.Serialization;

namespace MobileWebApi.Models
{
    public class PersonalDetailServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PersonalDetailResponseDto? Data { get; set; }
        /// <summary>
        /// SystemUserId for access control purposes only (not exposed in API response)
        /// </summary>
        [JsonIgnore]
        public int SystemUserId { get; set; }
    }

    public class PersonalDetailListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Employee>? Data { get; set; }
        public int TotalRecords { get; set; }
    }
}

