using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace MDRNJfitness.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IGameService _game;
        private readonly IWorkoutService _workout;
        private readonly IAuthService _auth;

        [ObservableProperty] private GameStatsDto? _stats;
        [ObservableProperty] private string _userName = string.Empty;

        public double XPProgress
        {
            get
            {
                if (Stats == null) return 0;
                int level = Stats.CurrentLevel;
                int xpStart = (level - 1) * (level - 1) * 100;
                int xpEnd = level * level * 100;
                int range = xpEnd - xpStart;
                return range <= 0 ? 0 : Math.Clamp((double)(Stats.TotalXP - xpStart) / range, 0, 1);
            }
        }

        partial void OnStatsChanged(GameStatsDto? value) => OnPropertyChanged(nameof(XPProgress));
        public ObservableCollection<WorkoutDto> RecentWorkouts { get; } = new();
        public ObservableCollection<AchievementDto> Achievements { get; } = new();

        [ObservableProperty] private bool _hasNoAchievements = true;

        public DashboardViewModel(IGameService game, IWorkoutService workout, IAuthService auth)
        {
            _game = game;
            _workout = workout;
            _auth = auth;
            Title = "Dashboard";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var user = await _auth.GetCurrentUserAsync();
                UserName = user?.Username ?? string.Empty;

                Stats = await _game.GetStatsAsync();

                await _game.CheckAchievementsAsync();
                var allAchievements = await _game.GetAchievementsAsync();
                Achievements.Clear();
                foreach (var a in allAchievements.Where(a => a.UnlockedAt.HasValue))
                    Achievements.Add(a);
                HasNoAchievements = Achievements.Count == 0;

                var history = await _workout.GetHistoryAsync(1, 5);
                RecentWorkouts.Clear();
                foreach (var w in history) RecentWorkouts.Add(w);
            });
    }
}
