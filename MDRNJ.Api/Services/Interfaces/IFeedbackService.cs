using MDRNJ.Shared.DTOs.Feedback;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task SubmitAsync(Guid userId, SubmitFeedbackDto dto);
        Task<List<FeedbackDto>> GetInboxAsync(bool includeResolved);
        Task SetResolvedAsync(Guid feedbackId, bool resolved);
    }
}
