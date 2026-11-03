using MDRNJ.Shared.Enums;

namespace MDRNJ.Shared.DTOs.Feedback
{
    /// <summary>A bug report or suggestion. Device fields are filled in by the app, not typed.</summary>
    public class SubmitFeedbackDto
    {
        public FeedbackKind Kind { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AppVersion { get; set; }
        public string? Platform { get; set; }
        public string? DeviceModel { get; set; }
    }
}
