using MDRNJ.Shared.DTOs.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameStatsDto> GetStatsAsync(Guid userId);
        Task<GameStatsDto> AwardXPAsync(Guid userId, int amount, string reason);
        Task<List<XPTransactionDto>> GetXPHistoryAsync(Guid userId);
    }
}
