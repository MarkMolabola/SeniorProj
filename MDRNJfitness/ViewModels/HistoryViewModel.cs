using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>One day's sessions under a TODAY / YESTERDAY / MON, OCT 27 header.</summary>
    public class HistoryDay
    {
        public required DateTime Date { get; init; }
        public required string Header { get; init; }
        public ObservableCollection<WorkoutDto> Sessions { get; } = new();
    }

    public partial class HistoryViewModel : BaseViewModel
    {
        private const int PageSize = 20;

        private readonly IWorkoutService _workout;
        private int _loadedPages;

        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private bool _hasNothing;

        public ObservableCollection<HistoryDay> Days { get; } = new();

        public HistoryViewModel(IWorkoutService workout)
        {
            _workout = workout;
            Title = "History";
        }

        /// <summary>Reloads from the newest session (on every visit, so a just-finished one shows).</summary>
        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var sessions = await _workout.GetHistoryAsync(1, PageSize);
                Days.Clear();
                _loadedPages = 1;
                Add(sessions);
                HasNothing = Days.Count == 0;
            });

        [RelayCommand]
        public async Task LoadMoreAsync() =>
            await RunSafeAsync(async () =>
            {
                var sessions = await _workout.GetHistoryAsync(_loadedPages + 1, PageSize);
                _loadedPages++;
                Add(sessions);
            });

        private void Add(List<WorkoutDto> sessions)
        {
            foreach (var session in sessions)
            {
                var date = session.StartTime.ToLocalTime().Date;
                var day = Days.LastOrDefault();
                if (day?.Date != date)
                {
                    day = new HistoryDay { Date = date, Header = Header(date) };
                    Days.Add(day);
                }
                day.Sessions.Add(session);
            }
            CanLoadMore = sessions.Count == PageSize;
        }

        private static string Header(DateTime date)
        {
            var today = DateTime.Today;
            if (date == today) return "TODAY";
            if (date == today.AddDays(-1)) return "YESTERDAY";
            return (date.Year == today.Year ? date.ToString("ddd, MMM d") : date.ToString("MMM d, yyyy"))
                .ToUpperInvariant();
        }
    }
}
