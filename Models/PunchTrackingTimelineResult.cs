using MobileWebApi.Models.Responses;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Result wrapper for punch tracking timeline retrieval with status.
    /// </summary>
    public class PunchTrackingTimelineResult
    {
        public PunchTrackingTimelineStatus Status { get; init; }
        public PunchTrackingTimelineResponse? Data { get; init; }

        public static PunchTrackingTimelineResult Unauthorized() =>
            new() { Status = PunchTrackingTimelineStatus.Unauthorized };

        public static PunchTrackingTimelineResult Forbidden() =>
            new() { Status = PunchTrackingTimelineStatus.Forbidden };

        public static PunchTrackingTimelineResult NotFound() =>
            new() { Status = PunchTrackingTimelineStatus.NotFound };

        public static PunchTrackingTimelineResult Success(PunchTrackingTimelineResponse data) =>
            new() { Status = PunchTrackingTimelineStatus.Success, Data = data };
    }

    public enum PunchTrackingTimelineStatus
    {
        Success,
        Unauthorized,
        Forbidden,
        NotFound
    }
}
