using MDRNJ.Shared.DTOs.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IAchievementService
    {
        Task<List<AchievementDto>> GetAllAsync(Guid userId);
        Task<List<AchievementDto>> GetUnlockedAsync(Guid userId);
        Task<List<AchievementDto>> CheckAndUnlockAsync(Guid userId);
    }
}
