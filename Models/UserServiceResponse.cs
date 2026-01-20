namespace MobileWebApi.Models
{
    public class UserServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? Data { get; set; }
    }

    public class UserListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<User>? Data { get; set; }
        public int TotalRecords { get; set; }
    }
}

