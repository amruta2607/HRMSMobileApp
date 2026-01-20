namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for deactivating/inactivating a user
    /// </summary>
    public class UserInactiveRequest
    {
        /// <summary>
        /// User ID to deactivate
        /// </summary>
        public int user_id { get; set; }
    }
}

