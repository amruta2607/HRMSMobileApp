namespace MobileWebApi.Models
{
	public class WithdrawLeaveRequest
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string? Reason { get; set; }
	}

}
