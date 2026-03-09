using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.ViewModels
{
    public class ChallengeViewModel : INotifyPropertyChanged
    {
        private readonly IChallengeService _challengeService;

        public ObservableCollection<Challenge> WeeklyChallenges { get; set; }

        public List<string> Difficulties { get; } = new List<string>
        {
            "Easy",
            "Medium",
            "Hard"
        };

        public bool IsDebugModeVisible { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        private string? _selectedDifficulty;
        public string? SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (_selectedDifficulty != value)
                {
                    _selectedDifficulty = value;
                    OnPropertyChanged();

                    if (!string.IsNullOrWhiteSpace(_selectedDifficulty) && CanSelectDifficulty)
                    {
                        LoadChallengesForWeek();
                    }
                }
            }
        }

        private bool _canSelectDifficulty = true;
        public bool CanSelectDifficulty
        {
            get => _canSelectDifficulty;
            set
            {
                if (_canSelectDifficulty != value)
                {
                    _canSelectDifficulty = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _weeklyResetTimer = "Next reset: calculating...";
        public string WeeklyResetTimer
        {
            get => _weeklyResetTimer;
            set
            {
                if (_weeklyResetTimer != value)
                {
                    _weeklyResetTimer = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ResetWeeklyChallengesCommand { get; }

        public ChallengeViewModel()
        {
            _challengeService = new ChallengeService();
            WeeklyChallenges = new ObservableCollection<Challenge>();

            ResetWeeklyChallengesCommand = new Command(ResetWeeklyChallenges);

            LoadExistingWeeklyChallenges();
            StartResetCountdown();
        }

        private void LoadExistingWeeklyChallenges()
        {
            var storedDifficulty = Preferences.Default.Get("weekly_difficulty", string.Empty);

            if (!string.IsNullOrWhiteSpace(storedDifficulty))
            {
                _selectedDifficulty = storedDifficulty;
                OnPropertyChanged(nameof(SelectedDifficulty));
                LoadChallengesForWeek();
            }
        }

        private void LoadChallengesForWeek()
        {
            if (string.IsNullOrWhiteSpace(SelectedDifficulty))
                return;

            WeeklyChallenges.Clear();

            var challenges = _challengeService.GetOrCreateWeeklyChallenges(SelectedDifficulty);

            foreach (var challenge in challenges)
            {
                WeeklyChallenges.Add(challenge);
            }

            CanSelectDifficulty = WeeklyChallenges.Count == 0;
        }

        private void ResetWeeklyChallenges()
        {
            _challengeService.ResetWeeklyChallengesForTesting();

            WeeklyChallenges.Clear();
            SelectedDifficulty = null;
            CanSelectDifficulty = true;
        }

        private void StartResetCountdown()
        {
            UpdateWeeklyResetTimer();

            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                UpdateWeeklyResetTimer();
                return true;
            });
        }

        private void UpdateWeeklyResetTimer()
        {
            DateTime now = DateTime.Now;
            DateTime nextReset = GetNextMondayAtNoon(now);

            TimeSpan remaining = nextReset - now;

            if (remaining.TotalSeconds < 0)
                remaining = TimeSpan.Zero;

            WeeklyResetTimer = $"Next weekly reset: {remaining.Days}d {remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s";
        }

        private DateTime GetNextMondayAtNoon(DateTime currentTime)
        {
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)currentTime.DayOfWeek + 7) % 7;

            DateTime nextMondayNoon = currentTime.Date.AddDays(daysUntilMonday).AddHours(12);

            if (currentTime >= nextMondayNoon)
            {
                nextMondayNoon = nextMondayNoon.AddDays(7);
            }

            return nextMondayNoon;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
