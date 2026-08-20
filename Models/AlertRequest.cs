namespace MobileWebApi.Models
{
	public class CreateAlertRequest
	{
		/// <summary>
		/// Organization ID (TenantId - foreign key to Tenant table)
		/// </summary>
		public int organization { get; set; }

		public int UserId { get; set; }
		public int? EventId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public string? Status { get; set; }
		public int? InsertUserId { get; set; }
	}

	public class UpdateAlertRequest
	{
		public int Id { get; set; }
		public string? Title { get; set; }
		public string? Message { get; set; }
		public bool? IsRead { get; set; }
		public bool? IsActive { get; set; }
		public string? Status { get; set; }
		public int? UpdateUserId { get; set; }
	}

	public class MarkAlertReadRequest
	{
		public int Id { get; set; }
		public int? UpdateUserId { get; set; }
	}

	public class ApproveAlertRequest
	{
		public int Id { get; set; }
		public int? UpdateUserId { get; set; }
	}

	/// <summary>
	/// Request model for approving request from alert (unified endpoint for mobile app)
	/// This will approve the underlying request AND update the alert status
	/// </summary>
	public class ApproveRequestFromAlertRequest
	{
		/// <summary>
		/// Alert ID
		/// </summary>
		public int AlertId { get; set; }

		// EventId and EventName are now always derived from the alert/event itself
	}

	/// <summary>
	/// Request model for rejecting request from alert (unified endpoint for mobile app)
	/// This will reject the underlying request AND update the alert status
	/// </summary>
	public class RejectRequestFromAlertRequest
	{
		/// <summary>
		/// Alert ID
		/// </summary>
		public int AlertId { get; set; }
	}

	public class RejectAlertRequest
	{
		public int Id { get; set; }
		public int? UpdateUserId { get; set; }
		public string? Reason { get; set; }
	}

	public class GetAlertsRequest
	{
		/// <summary>
		/// Organization ID (TenantId - foreign key to Tenant table)
		/// </summary>
		public int? organization { get; set; }

		public int? UserId { get; set; }
		public bool? IsRead { get; set; }
		public bool? IsActive { get; set; }
		public string? Status { get; set; }
	}

	/// <summary>
	/// Request model for sending approval notification to requester
	/// </summary>
	public class SendApprovalNotificationRequest
	{
		/// <summary>
		/// User ID of the person who made the original request (requester)
		/// </summary>
		public int RequesterUserId { get; set; }

		/// <summary>
		/// Event ID (e.g., LeaveRequestId, PayrollId, etc.)
		/// </summary>
		public int? EventId { get; set; }

		/// <summary>
		/// Event name (e.g., "LeaveRequest", "PayrollSubmission", "ReimbursementRequest", etc.)
		/// </summary>
		public string EventName { get; set; } = string.Empty;

		/// <summary>
		/// Title of the notification
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Message to send to the requester
		/// </summary>
		public string? Message { get; set; }

		/// <summary>
		/// Optional redirect URL for the notification
		/// </summary>
		public string? RedirectUrl { get; set; }
	}

	/// <summary>
	/// Request model for sending rejection notification to requester
	/// </summary>
	public class SendRejectionNotificationRequest
	{
		/// <summary>
		/// User ID of the person who made the original request (requester)
		/// </summary>
		public int RequesterUserId { get; set; }

		/// <summary>
		/// Event ID (e.g., LeaveRequestId, PayrollId, etc.)
		/// </summary>
		public int? EventId { get; set; }

		/// <summary>
		/// Event name (e.g., "LeaveRequest", "PayrollSubmission", "ReimbursementRequest", etc.)
		/// </summary>
		public string EventName { get; set; } = string.Empty;

		/// <summary>
		/// Title of the notification
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Message to send to the requester
		/// </summary>
		public string? Message { get; set; }

		/// <summary>
		/// Reason for rejection
		/// </summary>
		public string? Reason { get; set; }

		/// <summary>
		/// Optional redirect URL for the notification
		/// </summary>
		public string? RedirectUrl { get; set; }
	}
}



