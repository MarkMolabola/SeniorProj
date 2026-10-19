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
        partial void OnActivityScoreChanged(ActivityScoreDto? value)
        {
            UpdateStats();
            OnPropertyChanged(nameof(UnspentStatPoints));
            OnPropertyChanged(nameof(HasUnspentStatPoints));
            OnPropertyChanged(nameof(UnspentStatPointsText));
        }

        // ── Free stat points from levelling up ──
        public int UnspentStatPoints => ActivityScore?.UnspentStatPoints ?? 0;
        public bool HasUnspentStatPoints => UnspentStatPoints > 0;
        public string UnspentStatPointsText =>
            $"{UnspentStatPoints} FREE STAT POINT{(UnspentStatPoints == 1 ? "" : "S")} · TAP + TO SPEND";
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
            new(HeroAvatars.All.Select(a => new HeroAvatarOption(a)));

        /// <summary>The selected hero when it's a special one (comes dressed, no gear).</summary>
        [ObservableProperty] private HeroAvatar? _specialHero;
        public bool IsSpecialHero => SpecialHero is not null;
        partial void OnSpecialHeroChanged(HeroAvatar? value) => OnPropertyChanged(nameof(IsSpecialHero));

        public ObservableCollection<GearSlotCard> GearSlots { get; } =
            new(GearCatalog.DisplayOrder.Select(s => new GearSlotCard(s)));

        // ── Gear pop-up: owned items for the tapped slot ──
        private GearSlotCard? _pickerSlot;
        public ObservableCollection<CollectionCard> SlotChoices { get; } = new();
        [ObservableProperty] private string _pickerTitle = string.Empty;
        [ObservableProperty] private bool _hasSlotChoices;
        [ObservableProperty] private bool _pickerSlotHasItem;

        private int _gold;
        private bool _unlimitedGold;

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
            _gold = inventory.Gold;
            _unlimitedGold = inventory.UnlimitedGold;
            Show(inventory.Character);
        }

        private void Show(CharacterStateDto? state)
        {
            CharacterState = state;
            SpriteState = state?.OutfitSlot ?? "default";
            AvatarId = state?.AvatarId ?? HeroAvatars.Classic;
            SpecialHero = HeroAvatars.Find(AvatarId) is { IsSpecial: true } hero ? hero : null;

            // Gear stays saved for the Classic Hero, but special heroes wear only their own outfit
            var worn = state?.InDrawOrder() ?? Array.Empty<string>();
            HeroGear = SpecialHero is null ? worn : Array.Empty<string>();
            HasHiddenGear = HeroGear.Any(id => !HeroSprites.GearFits(id, AvatarId));
            UpdateStats();

            foreach (var slot in GearSlots)
            {
                slot.ItemId = state?.Get(slot.Slot);
                slot.Hero = SpecialHero;
            }
            foreach (var option in AvatarOptions)
            {
                option.IsSelected = option.Id == AvatarId;
                option.IsLocked = option.Hero.IsSpecial && !_owned.Contains(option.Id);
                option.PreviewOutfit = SpriteState;
                option.PreviewGear = worn;
            }
        }

        private void UpdateStats()
        {
            var gear = GearStats.Bonus(CharacterState, CharacterState?.AvatarId);
            StrengthPoints = (ActivityScore?.StrengthPoints ?? 0) + gear.Strength;
            EndurancePoints = (ActivityScore?.CardioPoints ?? 0) + gear.Endurance;
            FlexibilityPoints = (ActivityScore?.FlexibilityPoints ?? 0) + gear.Flexibility;

            var parts = new List<string>();
            if (gear.Strength > 0) parts.Add($"+{gear.Strength:0} STR");
            if (gear.Endurance > 0) parts.Add($"+{gear.Endurance:0} END");
            if (gear.Flexibility > 0) parts.Add($"+{gear.Flexibility:0} FLX");
            GearBonusText = parts.Count == 0 ? string.Empty
                : (SpecialHero is null ? "Gear: " : "Outfit: ") + string.Join(" · ", parts);
        }

        /// <summary>+ next to a stat: put one free point from levelling up into it.</summary>
        [RelayCommand]
        public async Task SpendStatPointAsync(string stat)
        {
            if (!HasUnspentStatPoints) return;
            await RunSafeAsync(async () => ActivityScore = await _character.SpendStatPointsAsync(stat));
        }

        /// <summary>CUSTOMIZE: pop-up grid of hero looks.</summary>
        [RelayCommand]
        public Task OpenCustomizeAsync() =>
            Shell.Current.Navigation.PushModalAsync(new AvatarPickerPage(this));

        [RelayCommand]
        public async Task SelectAvatarAsync(HeroAvatarOption option)
        {
            if (option.IsLocked && !await BuyHeroAsync(option.Hero))
                return;
            if (option.Id != AvatarId)
                await RunSafeAsync(async () => Show(await _character.SetAvatarAsync(option.Id)));
            await ClosePopupAsync();
        }

        /// <summary>Locked hero tapped: offer to buy it right here. True once it's owned.</summary>
        private async Task<bool> BuyHeroAsync(HeroAvatar hero)
        {
            // Asked from the CUSTOMIZE pop-up, so the dialogs go on top of it
            Page page = Shell.Current.Navigation.ModalStack.LastOrDefault() ?? Shell.Current;

            var pieces = string.Join("\n", hero.Pieces.Select(p =>
                $"• {p.Name} (+{p.Bonus} {GearStats.ShortName(p.Stat)})"));
            if (!_unlimitedGold && _gold < hero.Price)
            {
                await page.DisplayAlertAsync(hero.Name,
                    $"{hero.Rarity} hero · {hero.Price} gold (you have {_gold}).\n\nComes dressed in:\n{pieces}\n\n" +
                    "Train, finish quests and win duels to earn more gold!", "OK");
                return false;
            }
            if (!await page.DisplayAlertAsync($"Unlock the {hero.Name}?",
                    $"{hero.Rarity} hero · {(_unlimitedGold ? "free for admins" : $"{hero.Price} gold")}\n\nComes dressed in:\n{pieces}\n\n" +
                    "Special heroes wear their own outfit instead of gear. Your gear stays saved for the Classic Hero.",
                    "Unlock", "Cancel"))
                return false;

            var bought = false;
            await RunSafeAsync(async () =>
            {
                ShowInventory(await _gear.BuyAsync(hero.Id));
                bought = true;
            });
            return bought;
        }

        /// <summary>Gear slot tapped: pop-up of the owned items for that category.</summary>
        [RelayCommand]
        public async Task ChooseGearAsync(GearSlotCard slot)
        {
            if (SpecialHero is { } hero)
            {
                await Shell.Current.DisplayAlertAsync(slot.Piece?.Name ?? slot.Label,
                    (slot.Piece is { } piece ? $"Part of the {hero.Name}'s outfit: +{piece.Bonus} {GearStats.ShortName(piece.Stat)}.\n\n"
                                             : $"The {hero.Name} has nothing here.\n\n") +
                    "Special heroes come dressed. Switch to the Classic Hero (CUSTOMIZE) to wear your gear.", "OK");
                return;
            }

            _pickerSlot = slot;
            PickerTitle = slot.Label;
            PickerSlotHasItem = slot.ItemId is not null;

            SlotChoices.Clear();
            foreach (var item in GearCatalog.All.Where(i => i.Slot == slot.Slot && _owned.Contains(i.Id)))
                SlotChoices.Add(new CollectionCard(item) { Equipped = item.Id == slot.ItemId });
            HasSlotChoices = SlotChoices.Count > 0;

            await Shell.Current.Navigation.PushModalAsync(new GearPickerPage(this));
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
