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
        Task<ChallengeDto> SubmitAttemptAsync(Guid challengeId, float value);
        Task MarkResultSeenAsync(Guid challengeId);
    }
}
