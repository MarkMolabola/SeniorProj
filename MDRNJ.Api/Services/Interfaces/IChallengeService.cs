using MDRNJ.Shared.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IChallengeService
    {
        Task<ChallengeDto> CreateAsync(Guid userId, CreateChallengeDto dto);
        Task<ChallengeDto> JoinAsync(Guid userId, Guid challengeId);
        Task<List<ChallengeDto>> GetUserChallengesAsync(Guid userId);
        Task<List<ChallengeDto>> GetActiveAsync(Guid userId);
        Task<ChallengeDto> UpdateProgressAsync(Guid userId, Guid challengeId, float progress);
    }
}
