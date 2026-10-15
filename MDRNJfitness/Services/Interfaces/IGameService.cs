using MDRNJ.Shared.DTOs.Game;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameStatsDto> GetStatsAsync();
        Task CheckAchievementsAsync();
        Task<List<AchievementDto>> GetAchievementsAsync();
        Task<DungeonDto> GetDungeonAsync();
        Task<DungeonCollectDto> CollectDungeonAsync();
        Task<List<GoldTransactionDto>> GetGoldHistoryAsync(int count = 50);
    }
}
