using MDRNJ.Shared.DTOs.Game;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IDungeonService
    {
        Task<DungeonDto> GetAsync(Guid userId);
        Task<DungeonCollectDto> CollectAsync(Guid userId);
    }
}
