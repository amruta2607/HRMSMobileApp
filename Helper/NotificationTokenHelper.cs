using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MobileWebApi.Constants;
using MobileWebApi.Models;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Shared notification/email placeholder replacement, aligned with the Web HRMS
    /// <c>EventRepository.ExtractEventDetails</c> / <c>ReplaceTemplateTokens</c> logic.
    /// </summary>
    public static class NotificationTokenHelper
    {
        private static readonly JsonSerializerOptions EventDataJsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = false
        };

        /// <summary>
        /// Builds Events.EventData JSON for a regularization (EmployeeDispute) request.
        /// Property names match the Web HRMS EventData payload used by approval/notifications.
        /// </summary>
        public static string BuildRegularizationEventDataJson(
            EmployeeDispute dispute,
            int requestedUserId,
            int assignedApproverUserId)
        {
            var disputeDate = dispute.DisputeDate.ToString(StringConstants.EventDataDateFormat, CultureInfo.InvariantCulture);

            var eventData = new Dictionary<string, object?>
            {
                [StringConstants.JsonKeyDisputeId] = dispute.Id,
                [StringConstants.JsonKeyDisputeDate] = disputeDate,
                [StringConstants.JsonKeyEmployeeId] = dispute.EmployeeId,
                [StringConstants.JsonKeyDisputeCategoryId] = dispute.DisputeCategoryId,
                [StringConstants.JsonKeyPunchId] = dispute.PunchId,
                [StringConstants.JsonKeyRequestedPunchInTime] = FormatEventDateTime(dispute.RequestedPunchInTime),
                [StringConstants.JsonKeyRequestedPunchOutTime] = FormatEventDateTime(dispute.RequestedPunchOutTime),
                [StringConstants.JsonKeyReason] = dispute.Description ?? StringConstants.EmptyString,
                [StringConstants.JsonKeyRequestedUserId] = requestedUserId,
                // Assigned approver (reporting manager) at submission; overwritten with acting approver on action
                [StringConstants.JsonKeyApprovedUserId] = assignedApproverUserId,
                [StringConstants.JsonKeyApprovedBy] = StringConstants.EmptyString,
                [StringConstants.JsonKeyApprovalTimestamp] = StringConstants.EmptyString,
                [StringConstants.JsonKeyState] = EventStateConstants.Pending
            };

            return JsonSerializer.Serialize(eventData, EventDataJsonOptions);
        }

        /// <summary>
        /// Merges approval/rejection fields into existing Events.EventData (does not create a new event).
        /// </summary>
        public static string ApplyApprovalFieldsToEventDataJson(
            string? existingEventDataJson,
            int approvedUserId,
            string approvedBy,
            string state)
        {
            Dictionary<string, object?> eventData;

            if (!string.IsNullOrWhiteSpace(existingEventDataJson))
            {
                try
                {
                    eventData = JsonSerializer.Deserialize<Dictionary<string, object?>>(existingEventDataJson)
                        ?? new Dictionary<string, object?>();
                }
                catch
                {
                    eventData = new Dictionary<string, object?>();
                }
            }
            else
            {
                eventData = new Dictionary<string, object?>();
            }

            eventData[StringConstants.JsonKeyApprovedUserId] = approvedUserId;
            eventData[StringConstants.JsonKeyApprovedBy] = approvedBy ?? StringConstants.EmptyString;
            eventData[StringConstants.JsonKeyApprovalTimestamp] =
                DateTime.UtcNow.ToString(StringConstants.EventDataApprovalTimestampFormat, CultureInfo.InvariantCulture);
            eventData[StringConstants.JsonKeyState] = state;

            return JsonSerializer.Serialize(eventData, EventDataJsonOptions);
        }

        /// <summary>
        /// Builds the RegularizationDetails token from EmployeeDispute fields.
        /// Aligned with Web EventRepository.ExtractEventDetails.
        /// Example: "Regularization Date: 30-Jun-2026, Requested Punch Out: 19:00"
        /// Only includes parts that have values.
        /// </summary>
        public static string BuildRegularizationDetails(
            DateTime? disputeDate,
            DateTime? requestedPunchInTime,
            DateTime? requestedPunchOutTime)
        {
            var parts = new List<string>();

            if (disputeDate.HasValue && disputeDate.Value != default)
            {
                parts.Add($"Regularization Date: {disputeDate.Value.ToString(StringConstants.DateFormat, CultureInfo.InvariantCulture)}");
            }

            if (requestedPunchInTime.HasValue && requestedPunchInTime.Value != default)
            {
                parts.Add($"Requested Punch In: {requestedPunchInTime.Value:HH:mm}");
            }

            if (requestedPunchOutTime.HasValue && requestedPunchOutTime.Value != default)
            {
                parts.Add($"Requested Punch Out: {requestedPunchOutTime.Value:HH:mm}");
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Builds RegularizationDetails from EventData JSON fields when the dispute row is not available.
        /// Supports both snake_case and camelCase keys used by Mobile and Web.
        /// </summary>
        public static string BuildRegularizationDetailsFromEventData(JsonElement root)
        {
            DateTime? disputeDate = TryParseDateTime(GetString(root, StringConstants.JsonKeyDisputeDate, "disputeDate"));
            DateTime? punchIn = TryParseDateTime(GetString(root, StringConstants.JsonKeyRequestedPunchInTime, "requestedPunchInTime"));
            DateTime? punchOut = TryParseDateTime(GetString(root, StringConstants.JsonKeyRequestedPunchOutTime, "requestedPunchOutTime"));

            return BuildRegularizationDetails(disputeDate, punchIn, punchOut);
        }

        /// <summary>
        /// Adds RegularizationDetails / DisputeDate tokens in both {Token} and [Token] formats.
        /// </summary>
        public static void AddRegularizationTokens(
            Dictionary<string, string> tokenValues,
            string? regularizationDetails,
            string? disputeDateFormatted = null)
        {
            var details = regularizationDetails ?? StringConstants.EmptyString;
            tokenValues[StringConstants.TokenRegularizationDetails] = details;
            tokenValues[StringConstants.TokenRegularizationDetailsAlt] = details;

            if (!string.IsNullOrEmpty(disputeDateFormatted))
            {
                tokenValues[StringConstants.TokenDisputeDate] = disputeDateFormatted;
                tokenValues[StringConstants.TokenDisputeDateAlt] = disputeDateFormatted;
            }
        }

        /// <summary>
        /// Adds common person-name tokens in both brace and bracket formats.
        /// </summary>
        public static void AddPersonNameTokens(
            Dictionary<string, string> tokenValues,
            string? employeeName = null,
            string? approverName = null)
        {
            if (!string.IsNullOrEmpty(employeeName))
            {
                tokenValues[StringConstants.TokenUsername] = employeeName;
                tokenValues[StringConstants.TokenEmployeeName] = employeeName;
                tokenValues[StringConstants.TokenEmployeeNameBrace] = employeeName;
                tokenValues[StringConstants.TokenEmployeeNameBracketNoUnderscore] = employeeName;
            }

            if (!string.IsNullOrEmpty(approverName))
            {
                tokenValues[StringConstants.TokenApproverName] = approverName;
                tokenValues[StringConstants.TokenApproverNameAlt] = approverName;
            }
        }

        /// <summary>
        /// Replaces all known tokens in a template. Supports both {Token} and [Token] dictionary keys.
        /// </summary>
        public static string ReplaceTokens(string template, Dictionary<string, string> tokenValues)
        {
            if (string.IsNullOrEmpty(template) || tokenValues == null || tokenValues.Count == 0)
                return template;

            var result = new StringBuilder(template);
            foreach (var token in tokenValues)
            {
                if (string.IsNullOrEmpty(token.Key))
                    continue;

                result.Replace(token.Key, token.Value ?? StringConstants.EmptyString);
            }

            return result.ToString();
        }

        private static object FormatEventDateTime(DateTime? value)
        {
            if (!value.HasValue || value.Value == default)
                return StringConstants.EmptyString;

            return value.Value.ToString(StringConstants.EventDataDateTimeFormat, CultureInfo.InvariantCulture);
        }

        private static string? GetString(JsonElement root, string primaryKey, string secondaryKey)
        {
            if (root.TryGetProperty(primaryKey, out var primary))
            {
                if (primary.ValueKind == JsonValueKind.Null)
                    return null;

                var value = primary.ValueKind == JsonValueKind.String ? primary.GetString() : primary.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            if (root.TryGetProperty(secondaryKey, out var secondary))
            {
                if (secondary.ValueKind == JsonValueKind.Null)
                    return null;

                var value = secondary.ValueKind == JsonValueKind.String ? secondary.GetString() : secondary.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private static DateTime? TryParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
                return parsed;

            return null;
        }
    }
}
