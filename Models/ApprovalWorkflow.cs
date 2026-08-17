namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents an event in the approval workflow
    /// </summary>
    public class Event
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventTypeId { get; set; }
        public string? EventData { get; set; }
        public string? State { get; set; }
        public string? Status { get; set; }
        public int TenantId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
    }

    /// <summary>
    /// Represents an event type configuration
    /// </summary>
    public class EventType
    {
        public int Id { get; set; }
        public string? EventName { get; set; }
        public string? Description { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents an approval stage configuration
    /// </summary>
    public class ApprovalStage
    {
        public int Id { get; set; }
        public int EventTypeId { get; set; }
        public string? LevelName { get; set; }
        public int? WorkRoleId { get; set; }
        public string ExplicitUserIds { get; set; } // Single UserId from ApprovalStage.UserId
        public int StageOrder { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents an approval record
    /// </summary>
    public class Approval
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ApprovalStageId { get; set; }
        public int ApproverId { get; set; }
        public string? ApprovalStatus { get; set; } // Pending, Approved, Rejected
        public string? Comments { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int TenantId { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
    }

    /// <summary>
    /// Represents a screen notification
    /// </summary>
    public class ScreenNotification
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public int? EventId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
    }

    /// <summary>
    /// Email notification model
    /// </summary>
    public class EmailNotification
    {
        public int Id { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? Status { get; set; } // Pending, Sent, Failed
        public int TenantId { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Email template model
    /// </summary>
    public class EmailTemplate
    {
        public int Id { get; set; }
        public string? EventName { get; set; }
        public string? ActionType { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Notification template model (from Template table)
    /// </summary>
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Parameter { get; set; }
        public string? Format { get; set; }
        public string? TemplateType { get; set; }
        public string? ActionType { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Approver information
    /// </summary>
    public class ApproverInfo
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int? EmployeeId { get; set; }
    }

    /// <summary>
    /// Common constants for event types
    /// </summary>
    public static class EventConstants
    {
        public const string LeaveEvent = "LeaveRequest";
        public const string RegularizationEvent = "RegularizationRequest";
        
        public const string ApprovalStatusPending = "Pending";
        public const string ApprovalStatusApproved = "Approved";
        public const string ApprovalStatusRejected = "Rejected";
    }
}

