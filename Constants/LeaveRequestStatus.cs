using System.ComponentModel;

namespace MobileWebApi.Constants
{
	public enum LeaveRequestStatus
	{
		[Description("Submit")]
		Submit = 1,
		[Description("Approved")]
		Approve = 2,
		[Description("Rejected")]
		Reject = 3,
		[Description("Withdrawn")]
		Withdraw = 4,
		[Description("Canceled")]
		Canceled = 5,
		[Description("Pending")]
		Pending = 6,
		[Description("Pending For Approval")]
		PendingForApproval = 7,
		[Description("Cancellation Approved")]
		CancellationApproved = 8,
		[Description("Cancellation Rejected")]
		CancellationRejected = 9
	}
}
