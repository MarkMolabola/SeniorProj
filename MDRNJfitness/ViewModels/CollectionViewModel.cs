using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Everything the player owns; tap to wear or take off.</summary>
    public partial class CollectionViewModel : BaseViewModel
    {
        private readonly IGearService _gear;

        public ObservableCollection<CollectionCard> Items { get; } = new();

        [ObservableProperty] private bool _hasNothing;

        public CollectionViewModel(IGearService gear)
        {
            _gear = gear;
            Title = "Collection";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () => Show(await _gear.GetInventoryAsync()));

        private void Show(InventoryDto inventory)
        {
            var worn = inventory.Character.InDrawOrder().ToHashSet();
            Items.Clear();
            foreach (var id in inventory.OwnedItemIds)
                if (GearCatalog.Find(id) is GearItem item)
                    Items.Add(new CollectionCard(item) { Equipped = worn.Contains(id) });
            HasNothing = Items.Count == 0;
        }

        [RelayCommand]
        public async Task ToggleEquipAsync(CollectionCard card) =>
            await RunSafeAsync(async () =>
                Show(await _gear.EquipAsync(card.Item.Slot, card.Equipped ? null : card.Item.Id)));
    }
}
