using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IGearService
    {
        Task<InventoryDto> GetInventoryAsync();
        Task<InventoryDto> BuyAsync(string itemId);

        /// <summary>Wears an owned item, or empties the slot when itemId is null.</summary>
        Task<InventoryDto> EquipAsync(GearSlot slot, string? itemId);
    }
}
