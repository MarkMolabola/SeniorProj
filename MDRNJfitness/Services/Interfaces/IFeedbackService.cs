using MDRNJ.Shared.DTOs.Feedback;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task SubmitAsync(SubmitFeedbackDto dto);
        Task<List<FeedbackDto>> GetInboxAsync(bool includeResolved);
        Task ResolveAsync(Guid feedbackId);
        Task ReopenAsync(Guid feedbackId);
    }
}
