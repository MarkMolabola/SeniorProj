using MDRNJ.Shared.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    /// <summary>1v1 friend challenges (push-ups, pull-ups, plank, run).</summary>
    public interface IChallengeService
    {
        Task<ChallengeDto> CreateAsync(Guid userId, CreateChallengeDto dto);

        /// <summary>The user's challenges, newest first. Resolves any whose deadline passed.</summary>
        Task<List<ChallengeDto>> GetMyAsync(Guid userId);

        Task<ChallengeDto> AcceptAsync(Guid userId, Guid challengeId);
        Task<ChallengeDto> DeclineAsync(Guid userId, Guid challengeId);
        Task<ChallengeDto> CancelAsync(Guid userId, Guid challengeId);

        /// <summary>Records a result. Resolves the challenge once both best attempts are in.</summary>
        Task<ChallengeDto> SubmitAttemptAsync(Guid userId, Guid challengeId, float value);

        /// <summary>Marks the result as shown to the user, so the app stops popping it up.</summary>
        Task MarkResultSeenAsync(Guid userId, Guid challengeId);
    }
}
