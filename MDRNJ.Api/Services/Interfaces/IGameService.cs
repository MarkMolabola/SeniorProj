using MDRNJ.Shared.DTOs.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameStatsDto> GetStatsAsync(Guid userId);
        Task<List<XPTransactionDto>> GetXPHistoryAsync(Guid userId);

        /// <summary>The latest gold earned and spent, newest first.</summary>
        Task<List<GoldTransactionDto>> GetGoldHistoryAsync(Guid userId, int count);
    }
}
