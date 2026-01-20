namespace MobileWebApi.Models
{
    public class UserUpdateRequest
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string email_id { get; set; } = string.Empty;
        public string mobile_number { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string profile_pic { get; set; } = string.Empty;
        public int organisations { get; set; }

        public string WorkRoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}