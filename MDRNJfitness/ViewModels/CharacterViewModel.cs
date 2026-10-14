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
        private readonly IGameService _game;
        private List<string> _owned = new();

        [ObservableProperty] private CharacterStateDto? _characterState;
        [ObservableProperty] private ActivityScoreDto? _activityScore;
        [ObservableProperty] private GameStatsDto? _stats;

        // ── Account XP (moved here from Home) ──
        public double XPProgress
        {
            get
            {
                if (Stats is null) return 0;
                var start = Stats.CurrentLevel <= 1 ? 0 : AccountProgression.XPForNextLevel(Stats.CurrentLevel - 1);
                var end = AccountProgression.XPForNextLevel(Stats.CurrentLevel);
                return end <= start ? 0 : Math.Clamp((double)(Stats.TotalXP - start) / (end - start), 0, 1);
            }
        }

        partial void OnStatsChanged(GameStatsDto? value) => OnPropertyChanged(nameof(XPProgress));

        // ── Stats as they count in play: trained points + worn gear boosts ──
        [ObservableProperty] private float _strengthPoints;
        [ObservableProperty] private float _endurancePoints;
        [ObservableProperty] private float _flexibilityPoints;
        [ObservableProperty] private string _gearBonusText = string.Empty;
        public bool HasGearBonus => GearBonusText.Length > 0;
        partial void OnGearBonusTextChanged(string value) => OnPropertyChanged(nameof(HasGearBonus));
        partial void OnActivityScoreChanged(ActivityScoreDto? value) => UpdateStats();
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

        public CharacterViewModel(ICharacterService character, IGearService gear, IGameService game)
        {
            _character = character;
            _gear = gear;
            _game = game;
            Title = "Character";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                Stats = await _game.GetStatsAsync();
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
            UpdateStats();

            foreach (var slot in GearSlots)
                slot.ItemId = state?.Get(slot.Slot);
            foreach (var option in AvatarOptions)
            {
                option.IsSelected = option.Id == AvatarId;
                option.PreviewOutfit = SpriteState;
                option.PreviewGear = HeroGear;
            }
        }

        private void UpdateStats()
        {
            var gear = GearStats.Bonus(CharacterState);
            StrengthPoints = (ActivityScore?.StrengthPoints ?? 0) + gear.Strength;
            EndurancePoints = (ActivityScore?.CardioPoints ?? 0) + gear.Endurance;
            FlexibilityPoints = (ActivityScore?.FlexibilityPoints ?? 0) + gear.Flexibility;

            var parts = new List<string>();
            if (gear.Strength > 0) parts.Add($"+{gear.Strength:0} STR");
            if (gear.Endurance > 0) parts.Add($"+{gear.Endurance:0} END");
            if (gear.Flexibility > 0) parts.Add($"+{gear.Flexibility:0} FLX");
            GearBonusText = parts.Count == 0 ? string.Empty : "Gear: " + string.Join(" · ", parts);
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
