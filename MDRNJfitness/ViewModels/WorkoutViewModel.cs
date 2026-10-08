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

        [ObservableProperty] private WorkoutDto? _activeWorkout;
        [ObservableProperty] private string _workoutType = "Running";
        [ObservableProperty] private bool _hasActiveWorkout;
        [ObservableProperty] private string _elapsedTime = "0:00:00";

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
                StopStopwatch();

                var completed = await _workout.CompleteAsync(ActiveWorkout.SessionId);
                if (completed is not null)
                    History.Insert(0, completed);
                ActiveWorkout = null;
                HasActiveWorkout = false;
                ElapsedTime = "0:00:00";

                await _progress.PresentAsync(completed?.CharacterProgress, completed?.AccountProgress, "WORKOUT COMPLETE");
            });

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
