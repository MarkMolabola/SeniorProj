using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class WorkoutViewModel : BaseViewModel
    {
        private readonly IWorkoutService _workout;
        private readonly ICharacterProgressPresenter _progress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TracksDistance))]
        private WorkoutDto? _activeWorkout;
        [ObservableProperty] private string _workoutType = "Running";
        [ObservableProperty] private bool _hasActiveWorkout;
        [ObservableProperty] private string _elapsedTime = "0:00:00";
        [ObservableProperty] private string _distanceKm = string.Empty;

        /// <summary>Running, cycling, ...: ask for the distance when finishing.</summary>
        public bool TracksDistance => MDRNJ.Shared.Game.WorkoutTypes.TracksDistance(ActiveWorkout?.WorkoutType);

        public ObservableCollection<WorkoutDto> History { get; } = new();

        public IReadOnlyList<string> WorkoutTypes { get; } = MDRNJ.Shared.Game.WorkoutTypes.SelectableNames;

        private IDispatcherTimer? _stopwatchTimer;
        private DateTime _workoutStartedAt;

        public WorkoutViewModel(IWorkoutService workout, ICharacterProgressPresenter progress)
        {
            _workout = workout;
            _progress = progress;
            Title = "Workout";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var history = await _workout.GetHistoryAsync(1, 10);
                History.Clear();
                foreach (var w in history) History.Add(w);
            });

        [RelayCommand]
        public async Task StartWorkoutAsync() =>
            await RunSafeAsync(async () =>
            {
                ActiveWorkout = await _workout.StartAsync(new CreateWorkoutDto
                {
                    WorkoutType = WorkoutType,
                    StartTime = DateTime.UtcNow
                });
                HasActiveWorkout = true;
                StartStopwatch();
            });

        [RelayCommand]
        public async Task CompleteWorkoutAsync() =>
            await RunSafeAsync(async () =>
            {
                if (ActiveWorkout is null) return;
                if (!TryReadDistance(TracksDistance, DistanceKm, out var distance))
                {
                    await Shell.Current.DisplayAlertAsync("Distance", "Enter a distance in km, like 5 or 3.2, or leave it blank.", "OK");
                    return;
                }
                StopStopwatch();

                var completed = await _workout.CompleteAsync(ActiveWorkout.SessionId, distance);
                if (completed is not null)
                    History.Insert(0, completed);
                ActiveWorkout = null;
                HasActiveWorkout = false;
                ElapsedTime = "0:00:00";
                DistanceKm = string.Empty;

                await _progress.PresentAsync(completed?.CharacterProgress, completed?.AccountProgress, "WORKOUT COMPLETE");
            });

        /// <summary>Blank means no distance; anything else must be 0-1000 km.</summary>
        internal static bool TryReadDistance(bool tracksDistance, string text, out float? distanceKm)
        {
            distanceKm = null;
            if (!tracksDistance || string.IsNullOrWhiteSpace(text)) return true;
            if (!float.TryParse(text, out var km) || km < 0 || km > 1000) return false;
            distanceKm = km > 0 ? km : null;
            return true;
        }

        private void StartStopwatch()
        {
            _workoutStartedAt = DateTime.UtcNow;
            _stopwatchTimer = Application.Current!.Dispatcher.CreateTimer();
            _stopwatchTimer.Interval = TimeSpan.FromSeconds(1);
            _stopwatchTimer.Tick += (_, _) =>
            {
                var elapsed = DateTime.UtcNow - _workoutStartedAt;
                ElapsedTime = $"{(int)elapsed.TotalHours}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            };
            _stopwatchTimer.Start();
        }

        private void StopStopwatch()
        {
            _stopwatchTimer?.Stop();
            _stopwatchTimer = null;
        }
    }
}
