using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SelectWorkoutRoutine.Models;

namespace SelectWorkoutRoutine.ViewModels
{
    public class WorkoutViewModel : INotifyPropertyChanged
    {
        private string _selectedWorkout;

        // Collection of display names for Picker
        public ObservableCollection<string> Workouts { get; }

        public string SelectedWorkout
        {
            get => _selectedWorkout;
            set
            {
                if (_selectedWorkout != value)
                {
                    _selectedWorkout = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ConfirmWorkoutCommand { get; }

        public WorkoutViewModel()
        {
            // Convert enum to display names
            Workouts = new ObservableCollection<string>(
                Enum.GetValues(typeof(WorkoutRoutine))
                    .Cast<WorkoutRoutine>()
                    .Select(w => GetDisplayName(w))
            );

            ConfirmWorkoutCommand = new Command(async () => await NavigateToDetails());
        }

        private async Task NavigateToDetails()
        {
            if (string.IsNullOrWhiteSpace(SelectedWorkout))
                return;

            // Convert display name back to enum for navigation
            var routine = Enum.Parse<WorkoutRoutine>(RemoveSpaces(SelectedWorkout));

            await Shell.Current.GoToAsync(
                $"{nameof(SelectWorkoutRoutine.Views.WorkoutDetailPage)}?workout={routine}");
        }

        // Convert enum to display-friendly name
        public static string GetDisplayName(WorkoutRoutine routine)
        {
            return routine switch
            {
                WorkoutRoutine.UpperBody => "Upper Body",
                WorkoutRoutine.LegDay => "Leg Day",
                WorkoutRoutine.Cardio => "Cardio",
                WorkoutRoutine.StrengthTraining => "Strength Training",
                WorkoutRoutine.HighIntensityWorkout => "High Intensity Workout",
                WorkoutRoutine.SportsFitness => "Sports Fitness",
                _ => routine.ToString()
            };
        }

        // Convert display name back to enum string
        private static string RemoveSpaces(string displayName)
        {
            return displayName.Replace(" ", "");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}