using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MDRNJfitness.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string userName = "Melissa";

        [ObservableProperty]
        private int currentLevel = 3;

        [ObservableProperty]
        private int currentXP = 240;

        [ObservableProperty]
        private int xPToNextLevel = 60;

        [ObservableProperty]
        private int streak = 5;

        [ObservableProperty]
        private int todayXP = 40;

        public double XPProgress => (CurrentXP + XPToNextLevel) == 0
            ? 0
            : (double)CurrentXP / (CurrentXP + XPToNextLevel);

        public ObservableCollection<RecentWorkoutItem> RecentWorkouts { get; } = new();

        public DashboardViewModel()
        {
            LoadDashboard();
        }

        [RelayCommand]
        private void LoadDashboard()
        {
            RecentWorkouts.Clear();

            RecentWorkouts.Add(new RecentWorkoutItem
            {
                Name = "Leg Day",
                DateDisplay = "Today",
                DurationDisplay = "45 min"
            });

            RecentWorkouts.Add(new RecentWorkoutItem
            {
                Name = "Upper Body",
                DateDisplay = "Yesterday",
                DurationDisplay = "35 min"
            });

            RecentWorkouts.Add(new RecentWorkoutItem
            {
                Name = "Cardio Session",
                DateDisplay = "2 days ago",
                DurationDisplay = "30 min"
            });

            OnPropertyChanged(nameof(XPProgress));
        }

        [RelayCommand]
        private async Task StartWorkout()
        {
            // Replace with real navigation later
            await Shell.Current.DisplayAlert("Start Workout", "Navigate to WorkoutPage", "OK");
        }

        [RelayCommand]
        private async Task ViewCharacter()
        {
            // Replace with real navigation later
            await Shell.Current.DisplayAlert("Character", "Navigate to CharacterPage", "OK");
        }
    }

    public class RecentWorkoutItem
    {
        public string Name { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public string DurationDisplay { get; set; } = string.Empty;
    }
}