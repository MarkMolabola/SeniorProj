using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class GearService : IGearService
    {
        private readonly HttpClient _http;

        public GearService(HttpClient http) => _http = http;

        public Task<InventoryDto> GetInventoryAsync() =>
            _http.GetAsync<InventoryDto>("api/gear")!;

        public Task<InventoryDto> BuyAsync(string itemId) =>
            _http.PostAsync<InventoryDto>($"api/gear/{Uri.EscapeDataString(itemId)}/buy", new { })!;

        public Task<InventoryDto> EquipAsync(GearSlot slot, string? itemId) =>
            _http.PutAsync<InventoryDto>("api/gear/equip", new EquipGearDto { Slot = slot.ToString(), ItemId = itemId })!;
    }
}
