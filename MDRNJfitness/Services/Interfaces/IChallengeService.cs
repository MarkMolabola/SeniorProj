using MDRNJ.Shared.DTOs.Challenge;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IChallengeService
    {
        Task<List<ChallengeDto>> GetActiveChallengesAsync();
        Task<List<ChallengeDto>> GetMyChallengesAsync();
        Task<ChallengeDto> CreateAsync(CreateChallengeDto dto);
        Task<ChallengeDto> JoinAsync(Guid challengeId);
    }
}
