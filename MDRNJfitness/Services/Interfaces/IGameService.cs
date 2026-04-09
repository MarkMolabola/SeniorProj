using MDRNJ.Shared.DTOs.Game;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameStatsDto> GetStatsAsync();
        Task CheckAchievementsAsync();
        Task<List<AchievementDto>> GetAchievementsAsync();
    }
}
