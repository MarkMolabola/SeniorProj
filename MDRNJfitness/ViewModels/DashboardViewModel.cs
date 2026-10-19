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
        public string DungeonTitle => $"FLOOR {Dungeon?.Floor ?? 1} · {(Dungeon?.MonsterName ?? "Slime").ToUpperInvariant()}S";

        // Floor progress: kills banked plus the ones the current run will add when collected
        public int PendingKills => Dungeon is null ? 0
            : DungeonRules.PendingKills(Dungeon.Power, TimeSpan.FromSeconds(Dungeon.SecondsAccrued) + _sinceDungeonFetch.Elapsed);
        public bool IsBottomFloor => Dungeon is { KillsToClear: 0 };
        public double FloorProgress => Dungeon is { KillsToClear: > 0 } d
            ? Math.Clamp((double)(d.FloorKills + PendingKills) / d.KillsToClear, 0, 1) : 1;
        public string FloorText => Dungeon is not { } d ? string.Empty
            : IsBottomFloor ? $"Deepest floor · ×{d.GoldMultiplier:0.00} gold"
            : d.FloorKills + PendingKills >= d.KillsToClear && d.NextFloorMinPower is float need
                ? $"Floor cleared! Reach power {need:0.#} to go deeper"
            : $"{Math.Min(d.FloorKills + PendingKills, d.KillsToClear)} / {d.KillsToClear} kills · ×{d.GoldMultiplier:0.00} gold";

        // Weekly boss
        public string BossTitle => $"WEEKLY BOSS · {(Dungeon?.BossName ?? string.Empty).ToUpperInvariant()}";
        public double BossHealth => Dungeon is { BossHp: > 0 } d ? Math.Clamp(1 - (double)d.BossDamage / d.BossHp, 0, 1) : 0;
        public string BossText => Dungeon is not { } d ? string.Empty
            : d.BossDefeated ? $"Defeated! A new boss arrives in {Left(d.BossEndsAt)}."
            : d.Power < d.BossMinPower ? $"Needs power {d.BossMinPower:0.#} to fight · leaves in {Left(d.BossEndsAt)}"
            : $"{d.BossHp - d.BossDamage} / {d.BossHp} HP · your kills hit it on COLLECT · {Left(d.BossEndsAt)} left";

        private static string Left(DateTime until)
        {
            var left = DateTime.SpecifyKind(until, DateTimeKind.Utc) - DateTime.UtcNow;
            return left.TotalDays >= 1 ? $"{(int)left.TotalDays}d {left.Hours}h"
                : left.TotalHours >= 1 ? $"{(int)left.TotalHours}h {left.Minutes}m"
                : $"{Math.Max(1, (int)left.TotalMinutes)}m";
        }
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
            OnPropertyChanged(nameof(IsBottomFloor));
            OnPropertyChanged(nameof(BossTitle));
            OnPropertyChanged(nameof(BossHealth));
            OnPropertyChanged(nameof(BossText));
            OnPropertyChanged(nameof(PowerText));
            UpdatePendingGold();
        }

        partial void OnPendingGoldChanged(int value)
        {
            OnPropertyChanged(nameof(FloorProgress));
            OnPropertyChanged(nameof(FloorText));
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
            DungeonCollectDto? loot = null;
            await RunSafeAsync(async () =>
            {
                var result = await _game.CollectDungeonAsync();
                loot = result;
                collected = result.Collected;
                ShowDungeon(result.Dungeon);
                if (Stats is not null)
                {
                    Stats.Gold = result.Gold;
                    OnPropertyChanged(nameof(GoldText));
                }
            });
            if (loot is not null)
                await ShowLootAsync(loot);
            if (collected == 0) return;

            CollectMessage = $"+{collected} gold · {loot?.Kills ?? 0} kills";
            await Task.Delay(2500);
            CollectMessage = string.Empty;
        }

        /// <summary>Anything bigger than gold from a collect: a new floor, boss damage or kill, a drop.</summary>
        private static async Task ShowLootAsync(DungeonCollectDto loot)
        {
            var lines = new List<string>();
            if (loot.NewFloor is int floor)
            {
                var f = DungeonRules.Floor(floor);
                lines.Add($"▼ You reached floor {floor}: {f.MonsterName}s, ×{f.GoldMultiplier:0.00} gold.");
            }
            if (loot.BossDefeated)
                lines.Add($"★ You defeated the weekly boss, {loot.Dungeon.BossName}! +{loot.BossGold} gold.");
            else if (loot.BossDamage > 0)
                lines.Add($"⚔ {loot.BossDamage} damage to {loot.Dungeon.BossName} ({loot.Dungeon.BossHp - loot.Dungeon.BossDamage} HP left).");
            if (GearCatalog.Find(loot.DroppedItemId) is { } item)
                lines.Add($"✦ DROP: {item.Name} ({item.Rarity}, {item.BonusText}). It's in your Collection, and the shop never sells it!");

            // Boss damage alone isn't worth a pop-up; it shows on the boss bar
            if (loot.NewFloor is null && !loot.BossDefeated && loot.DroppedItemId is null)
                return;

            var title = loot.DroppedItemId is not null ? "RARE DROP!" : loot.BossDefeated ? "BOSS DEFEATED!" : "DEEPER!";
            await Shell.Current.DisplayAlertAsync(title, string.Join("\n\n", lines), "NICE");
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
            PendingGold = DungeonRules.PendingGold(Dungeon.Power, elapsed, Dungeon.Floor);
        }

        private void ShowDungeon(DungeonDto dungeon)
        {
            _sinceDungeonFetch.Restart();
            Dungeon = dungeon;
        }
    }
}
