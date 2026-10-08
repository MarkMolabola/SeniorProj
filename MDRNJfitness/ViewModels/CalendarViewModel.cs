using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MDRNJfitness.ViewModels
{
    public partial class CalendarViewModel : BaseViewModel
    {
        private readonly IWorkoutService _workout;
        private readonly ICharacterProgressPresenter _progress;

        [ObservableProperty] private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        [ObservableProperty] private string _monthLabel = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedDay))]
        private CalendarDayItem? _selectedDay;

        public bool HasSelectedDay => SelectedDay is not null;

        // Backlog form fields
        [ObservableProperty] private string _backlogWorkoutType = "Running";
        [ObservableProperty] private TimeSpan _backlogTime = TimeSpan.FromHours(8);
        [ObservableProperty] private int _backlogDuration = 30;

        public IReadOnlyList<string> WorkoutTypes { get; } = MDRNJ.Shared.Game.WorkoutTypes.SelectableNames;

        // Badge state — recalculated after every load
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFiveDayStreakBadge))]
        private int _longestStreak;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTenDayBadge))]
        [NotifyPropertyChangedFor(nameof(HasTwentyDayBadge))]
        private int _totalWorkoutDays;

        public bool HasFiveDayStreakBadge => LongestStreak >= 5;
        public bool HasTenDayBadge        => TotalWorkoutDays >= 10;
        public bool HasTwentyDayBadge     => TotalWorkoutDays >= 20;

        public ObservableCollection<CalendarDayItem> Days { get; } = new();
        public ObservableCollection<WorkoutDto> SelectedWorkouts { get; } = new();

        private List<WorkoutDto> _monthWorkouts = new();

        public CalendarViewModel(IWorkoutService workout, ICharacterProgressPresenter progress)
        {
            _workout = workout;
            _progress = progress;
            Title = "Calendar";
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            await RunSafeAsync(async () =>
            {
                _monthWorkouts = await _workout.GetMonthlyAsync(DisplayMonth.Year, DisplayMonth.Month)
                                 ?? new List<WorkoutDto>();
                BuildGrid();
            });
        }

        [RelayCommand]
        public async Task PreviousMonthAsync()
        {
            DisplayMonth = DisplayMonth.AddMonths(-1);
            await LoadAsync();
        }

        [RelayCommand]
        public async Task NextMonthAsync()
        {
            DisplayMonth = DisplayMonth.AddMonths(1);
            await LoadAsync();
        }

        [RelayCommand]
        public void SelectDay(CalendarDayItem day)
        {
            if (SelectedDay is not null)
                SelectedDay.IsSelected = false;

            day.IsSelected = true;
            SelectedDay = day;

            SelectedWorkouts.Clear();
            foreach (var w in _monthWorkouts
                .Where(w => w.StartTime.ToLocalTime().Date == day.Date.Date))
                SelectedWorkouts.Add(w);
        }

        [RelayCommand]
        public async Task LogBacklogAsync()
        {
            if (SelectedDay is null) return;

            await RunSafeAsync(async () =>
            {
                var startLocal = SelectedDay.Date.Date + BacklogTime;
                var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();

                var added = await _workout.BacklogAsync(new BacklogWorkoutDto
                {
                    WorkoutType = BacklogWorkoutType,
                    StartTime = startUtc,
                    DurationMins = BacklogDuration
                });

                _monthWorkouts.Add(added);
                SelectedWorkouts.Add(added);

                // Update dot indicator on the calendar cell
                var cell = Days.FirstOrDefault(d => d.Date.Date == SelectedDay.Date.Date);
                if (cell is not null)
                    cell.HasWorkout = true;

                await _progress.PresentAsync(added.CharacterProgress, added.AccountProgress, "WORKOUT LOGGED");
            });
        }

        private void BuildGrid()
        {
            MonthLabel = DisplayMonth.ToString("MMMM yyyy");
            Days.Clear();
            SelectedWorkouts.Clear();
            SelectedDay = null;

            var workoutDates = _monthWorkouts
                .Select(w => w.StartTime.ToLocalTime().Date)
                .ToHashSet();

            // Grid starts on Sunday of the week containing the 1st
            var first = DisplayMonth;
            var gridStart = first.AddDays(-(int)first.DayOfWeek);

            // 6 rows × 7 cols = 42 cells
            for (int i = 0; i < 42; i++)
            {
                var date = gridStart.AddDays(i);
                Days.Add(new CalendarDayItem
                {
                    Date = date,
                    IsCurrentMonth = date.Month == DisplayMonth.Month,
                    HasWorkout = workoutDates.Contains(date.Date)
                });
            }

            CalculateBadges(workoutDates);
        }

        private void CalculateBadges(HashSet<DateTime> workoutDates)
        {
            // Only count days within the displayed month
            var monthDates = workoutDates
                .Where(d => d.Month == DisplayMonth.Month && d.Year == DisplayMonth.Year)
                .OrderBy(d => d)
                .ToList();

            TotalWorkoutDays = monthDates.Count;

            // Longest consecutive streak within the month
            int max = 0, current = 0;
            for (int i = 0; i < monthDates.Count; i++)
            {
                if (i == 0 || (monthDates[i] - monthDates[i - 1]).Days == 1)
                    current++;
                else
                    current = 1;

                if (current > max) max = current;
            }
            LongestStreak = max;
        }
    }
}
