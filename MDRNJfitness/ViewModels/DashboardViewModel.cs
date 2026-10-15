using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJ.Shared.Game;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MDRNJfitness.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IGameService _game;
        private readonly IWorkoutService _workout;
        private readonly ICharacterService _character;

        /// <summary>Time since the dungeon snapshot was fetched, so the gold counter keeps ticking.</summary>
        private readonly Stopwatch _sinceDungeonFetch = new();

        [ObservableProperty] private GameStatsDto? _stats;
        [ObservableProperty] private string _heroOutfit = "default";
        [ObservableProperty] private string _heroAvatarId = "classic";
        [ObservableProperty] private MDRNJ.Shared.DTOs.Character.CharacterStateDto? _hero;

        // ── Dungeon ─────────────────────────────────────────────
        [ObservableProperty] private DungeonDto? _dungeon;
        [ObservableProperty] private int _pendingGold;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCollectMessage))]
        private string _collectMessage = string.Empty;
        public bool HasCollectMessage => CollectMessage.Length > 0;

        public string MonsterId => Dungeon?.MonsterId ?? "slime";
        public string DungeonTitle => $"DUNGEON · {(Dungeon?.MonsterName ?? "Slime").ToUpperInvariant()}S";
        public string PowerText => $"POWER {Dungeon?.Power ?? 3:0.#}";
        public string RateText => Dungeon is null ? string.Empty
            : IsDungeonFull ? "Full! Collect to keep earning."
            : $"+{Dungeon.GoldPerHour:0.#} gold/hr · holds {Dungeon.CapHours}h";
        public string PendingText => $"{PendingGold} / {Dungeon?.CapGold ?? 0}";
        public double PendingProgress => Dungeon is { CapGold: > 0 } d ? Math.Clamp((double)PendingGold / d.CapGold, 0, 1) : 0;
        public bool IsDungeonFull => Dungeon is { CapGold: > 0 } d && PendingGold >= d.CapGold;
        public bool CanCollect => PendingGold > 0;

        partial void OnStatsChanged(GameStatsDto? value) => OnPropertyChanged(nameof(GoldText));

        partial void OnDungeonChanged(DungeonDto? value)
        {
            OnPropertyChanged(nameof(MonsterId));
            OnPropertyChanged(nameof(DungeonTitle));
            OnPropertyChanged(nameof(PowerText));
            UpdatePendingGold();
        }

        partial void OnPendingGoldChanged(int value)
        {
            OnPropertyChanged(nameof(PendingText));
            OnPropertyChanged(nameof(PendingProgress));
            OnPropertyChanged(nameof(IsDungeonFull));
            OnPropertyChanged(nameof(RateText));
            OnPropertyChanged(nameof(CanCollect));
        }

        /// <summary>Gold balance, or ∞ for admins (who shop for free).</summary>
        public string GoldText => Stats is null ? "0" : Stats.UnlimitedGold ? "∞" : Stats.Gold.ToString();
        public ObservableCollection<WorkoutDto> RecentWorkouts { get; } = new();
        public ObservableCollection<AchievementDto> Achievements { get; } = new();

        [ObservableProperty] private bool _hasNoAchievements = true;

        private readonly NotificationHubClient _notifications;

        public DashboardViewModel(IGameService game, IWorkoutService workout, ICharacterService character,
            NotificationHubClient notifications)
        {
            _game = game;
            _workout = workout;
            _character = character;
            _notifications = notifications;
            Title = "Dashboard";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                Stats = await _game.GetStatsAsync();

                var hero = await _character.GetCharacterAsync();
                HeroOutfit = hero?.OutfitSlot ?? "default";
                HeroAvatarId = hero?.AvatarId ?? "classic";
                Hero = hero;

                await _game.CheckAchievementsAsync();
                var allAchievements = await _game.GetAchievementsAsync();
                Achievements.Clear();
                foreach (var a in allAchievements.Where(a => a.UnlockedAt.HasValue))
                    Achievements.Add(a);
                HasNoAchievements = Achievements.Count == 0;

                // After achievements: their payouts and gear changes can raise power
                ShowDungeon(await _game.GetDungeonAsync());

                var history = await _workout.GetHistoryAsync(1, 5);
                RecentWorkouts.Clear();
                foreach (var w in history) RecentWorkouts.Add(w);

                // Signed in and the token is fresh: start live notifications (no-op if already on)
                _ = _notifications.ConnectAsync();
            });

        [RelayCommand]
        public Task OpenGoldHistoryAsync() => Shell.Current.GoToAsync("GoldHistoryPage");

        [RelayCommand]
        public async Task CollectAsync()
        {
            var collected = 0;
            await RunSafeAsync(async () =>
            {
                var result = await _game.CollectDungeonAsync();
                collected = result.Collected;
                ShowDungeon(result.Dungeon);
                if (Stats is not null)
                {
                    Stats.Gold = result.Gold;
                    OnPropertyChanged(nameof(GoldText));
                }
            });
            if (collected == 0) return;

            CollectMessage = $"+{collected} gold!";
            await Task.Delay(2500);
            CollectMessage = string.Empty;
        }

        /// <summary>Called every second while Home is open.</summary>
        public void UpdatePendingGold()
        {
            if (Dungeon is null)
            {
                PendingGold = 0;
                return;
            }
            var elapsed = TimeSpan.FromSeconds(Dungeon.SecondsAccrued) + _sinceDungeonFetch.Elapsed;
            PendingGold = DungeonRules.PendingGold(Dungeon.Power, elapsed);
        }

        private void ShowDungeon(DungeonDto dungeon)
        {
            _sinceDungeonFetch.Restart();
            Dungeon = dungeon;
        }
    }
}
