using MDRNJ.Shared.DTOs.Game;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    /// <summary>Gold, the gear shop, and what the hero wears.</summary>
    public interface IGearService
    {
        Task<InventoryDto> GetInventoryAsync(Guid userId);

        /// <summary>Spends gold on a catalog item. Fails if already owned or too expensive.</summary>
        Task<InventoryDto> BuyAsync(Guid userId, string itemId);

        /// <summary>Wears an owned item in its slot, or empties the slot when itemId is null.</summary>
        Task<InventoryDto> EquipAsync(Guid userId, string slot, string? itemId);
    }
}
