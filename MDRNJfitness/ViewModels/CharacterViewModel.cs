using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Sprites;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.Views;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class CharacterViewModel : BaseViewModel
    {
        private readonly ICharacterService _character;
        private readonly IGearService _gear;
        private List<string> _owned = new();

        [ObservableProperty] private CharacterStateDto? _characterState;
        [ObservableProperty] private ActivityScoreDto? _activityScore;
        [ObservableProperty] private string _avatarId = HeroAvatars.Classic;
        [ObservableProperty] private IReadOnlyList<string> _heroGear = Array.Empty<string>();

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

        // ── Gear pop-up: owned items for the tapped slot ──
        private GearSlotCard? _pickerSlot;
        public ObservableCollection<CollectionCard> SlotChoices { get; } = new();
        [ObservableProperty] private string _pickerTitle = string.Empty;
        [ObservableProperty] private bool _hasSlotChoices;
        [ObservableProperty] private bool _pickerSlotHasItem;

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

        /// <summary>CUSTOMIZE: pop-up grid of hero looks.</summary>
        [RelayCommand]
        public Task OpenCustomizeAsync() =>
            Shell.Current.Navigation.PushModalAsync(new AvatarPickerPage(this));

        [RelayCommand]
        public async Task SelectAvatarAsync(HeroAvatarOption option)
        {
            if (option.Id != AvatarId)
                await RunSafeAsync(async () => Show(await _character.SetAvatarAsync(option.Id)));
            await ClosePopupAsync();
        }

        /// <summary>Gear slot tapped: pop-up of the owned items for that category.</summary>
        [RelayCommand]
        public Task ChooseGearAsync(GearSlotCard slot)
        {
            _pickerSlot = slot;
            PickerTitle = slot.Label;
            PickerSlotHasItem = slot.ItemId is not null;

            SlotChoices.Clear();
            foreach (var item in GearCatalog.All.Where(i => i.Slot == slot.Slot && _owned.Contains(i.Id)))
                SlotChoices.Add(new CollectionCard(item) { Equipped = item.Id == slot.ItemId });
            HasSlotChoices = SlotChoices.Count > 0;

            return Shell.Current.Navigation.PushModalAsync(new GearPickerPage(this));
        }

        /// <summary>Tap an item: wear it, or take it off if it's already worn.</summary>
        [RelayCommand]
        public async Task WearChoiceAsync(CollectionCard card)
        {
            if (_pickerSlot is null) return;
            var itemId = card.Equipped ? null : card.Item.Id;
            await RunSafeAsync(async () => ShowInventory(await _gear.EquipAsync(_pickerSlot.Slot, itemId)));
            await ClosePopupAsync();
        }

        [RelayCommand]
        public async Task TakeOffAsync()
        {
            if (_pickerSlot is null) return;
            await RunSafeAsync(async () => ShowInventory(await _gear.EquipAsync(_pickerSlot.Slot, null)));
            await ClosePopupAsync();
        }

        [RelayCommand]
        public async Task VisitShopAsync()
        {
            await ClosePopupAsync();
            await Shell.Current.GoToAsync("//ShopPage");
        }

        [RelayCommand]
        public async Task ClosePopupAsync()
        {
            if (Shell.Current.Navigation.ModalStack.Count > 0)
                await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
