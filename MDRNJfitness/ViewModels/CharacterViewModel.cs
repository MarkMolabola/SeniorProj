using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Sprites;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class CharacterViewModel : BaseViewModel
    {
        private const string RemoveOption = "Take it off";

        private readonly ICharacterService _character;
        private readonly IGearService _gear;
        private List<string> _owned = new();

        [ObservableProperty] private CharacterStateDto? _characterState;
        [ObservableProperty] private ActivityScoreDto? _activityScore;
        [ObservableProperty] private string _avatarId = HeroAvatars.Classic;
        [ObservableProperty] private IReadOnlyList<string> _heroGear = Array.Empty<string>();
        [ObservableProperty] private bool _isCustomizing;

        /// <summary>Worn gear that has no art for the current hero yet (so it isn't drawn).</summary>
        [ObservableProperty] private bool _hasHiddenGear;

        private string _spriteState = "default";
        public string SpriteState
        {
            get => _spriteState;
            set { _spriteState = value; OnPropertyChanged(); }
        }

        public ObservableCollection<HeroAvatarOption> AvatarOptions { get; } =
            new(HeroAvatars.All.Select(a => new HeroAvatarOption(a.Id, a.Name)));

        public ObservableCollection<GearSlotCard> GearSlots { get; } =
            new(GearCatalog.DisplayOrder.Select(s => new GearSlotCard(s)));

        public CharacterViewModel(ICharacterService character, IGearService gear)
        {
            _character = character;
            _gear = gear;
            Title = "Character";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                ActivityScore = await _character.GetActivityScoreAsync();
                ShowInventory(await _gear.GetInventoryAsync());
            });

        private void ShowInventory(InventoryDto inventory)
        {
            _owned = inventory.OwnedItemIds;
            Show(inventory.Character);
        }

        private void Show(CharacterStateDto? state)
        {
            CharacterState = state;
            SpriteState = state?.OutfitSlot ?? "default";
            AvatarId = state?.AvatarId ?? HeroAvatars.Classic;
            HeroGear = state?.InDrawOrder() ?? Array.Empty<string>();
            HasHiddenGear = HeroGear.Any(id => !HeroSprites.GearFits(id, AvatarId));

            foreach (var slot in GearSlots)
                slot.ItemId = state?.Get(slot.Slot);
            foreach (var option in AvatarOptions)
            {
                option.IsSelected = option.Id == AvatarId;
                option.PreviewOutfit = SpriteState;
                option.PreviewGear = HeroGear;
            }
        }

        [RelayCommand]
        public void ToggleCustomize() => IsCustomizing = !IsCustomizing;

        [RelayCommand]
        public async Task SelectAvatarAsync(HeroAvatarOption option)
        {
            if (option.Id == AvatarId) return;

            await RunSafeAsync(async () => Show(await _character.SetAvatarAsync(option.Id)));
        }

        /// <summary>Pick which owned item to wear in a slot (or take the current one off).</summary>
        [RelayCommand]
        public async Task ChooseGearAsync(GearSlotCard slot)
        {
            var choices = GearCatalog.All
                .Where(i => i.Slot == slot.Slot && _owned.Contains(i.Id))
                .ToList();

            if (choices.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync($"No {slot.Label.ToLowerInvariant()} gear yet",
                    "Buy gear in the Shop on the Home tab, then equip it here.", "OK");
                return;
            }

            var names = choices.Select(i => i.Id == slot.ItemId ? $"{i.Name} (wearing)" : i.Name).ToList();
            if (slot.ItemId is not null) names.Add(RemoveOption);

            var picked = await Shell.Current.DisplayActionSheetAsync(slot.Label, "Cancel", null, names.ToArray());
            if (picked is null || picked == "Cancel") return;

            var itemId = picked == RemoveOption ? null : choices[names.IndexOf(picked)].Id;
            if (itemId == slot.ItemId) return;

            await RunSafeAsync(async () => ShowInventory(await _gear.EquipAsync(slot.Slot, itemId)));
        }
    }
}
