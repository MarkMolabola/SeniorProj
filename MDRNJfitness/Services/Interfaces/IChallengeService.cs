using MDRNJ.Shared.DTOs.Challenge;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IChallengeService
    {
        Task<List<ChallengeDto>> GetMyChallengesAsync();
        Task<ChallengeDto> CreateAsync(CreateChallengeDto dto);
        Task<ChallengeDto> AcceptAsync(Guid challengeId);
        Task<ChallengeDto> DeclineAsync(Guid challengeId);
        Task<ChallengeDto> CancelAsync(Guid challengeId);
        Task<ChallengeDto> SubmitAttemptAsync(Guid challengeId, float value, string? proofPath = null);
        Task MarkResultSeenAsync(Guid challengeId);

        /// <summary>Uploads a proof video / photo; returns the path to submit with the result.</summary>
        Task<string> UploadProofAsync(Guid challengeId, FileResult file);

        /// <summary>A short-lived link to view a proof.</summary>
        Task<Uri> GetProofLinkAsync(Guid challengeId, Guid attemptId);

        Task<ChallengeDto> ApproveAsync(Guid challengeId);
        Task<ChallengeDto> DisputeAsync(Guid challengeId, string? reason);
    }
}
