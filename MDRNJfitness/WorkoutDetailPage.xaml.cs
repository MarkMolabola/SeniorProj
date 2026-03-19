using SelectWorkoutRoutine.Models;
using SelectWorkoutRoutine.ViewModels;

namespace SelectWorkoutRoutine.Views;

[QueryProperty(nameof(Workout), "workout")]
public partial class WorkoutDetailPage : ContentPage
{
    public WorkoutDetailPage()
    {
        InitializeComponent();
    }

    // The enum string is passed via Shell query parameter
    public string Workout
    {
        set
        {
            LoadWorkoutDetails(value);
        }
    }

    private void LoadWorkoutDetails(string workoutType)
    {
        // Display name with spaces
        WorkoutTitle.Text = WorkoutViewModel.GetDisplayName(
            Enum.Parse<WorkoutRoutine>(workoutType)
        );

        TasksLayout.Children.Clear();

        string description = "";
        List<string> tasks = new();

        switch (workoutType)
        {
            case nameof(WorkoutRoutine.UpperBody):
                description = "Focuses on chest, shoulders, arms, and back.";
                tasks.AddRange(new[]
                {
                    "Push-ups - 3 sets of 15",
                    "Bench Press - 4 sets of 10",
                    "Shoulder Press - 3 sets of 12",
                    "Bicep Curls - 3 sets of 15"
                });
                break;

            case nameof(WorkoutRoutine.LegDay):
                description = "Targets quads, hamstrings, glutes, and calves.";
                tasks.AddRange(new[]
                {
                    "Squats - 4 sets of 12",
                    "Lunges - 3 sets each leg",
                    "Leg Press - 3 sets of 12",
                    "Calf Raises - 4 sets of 20"
                });
                break;

            case nameof(WorkoutRoutine.Cardio):
                description = "Improves endurance and heart health.";
                tasks.AddRange(new[]
                {
                    "Running - 20 minutes",
                    "Jump Rope - 10 minutes",
                    "Cycling - 15 minutes"
                });
                break;

            case nameof(WorkoutRoutine.StrengthTraining):
                description = "Builds total body strength.";
                tasks.AddRange(new[]
                {
                    "Deadlifts - 4 sets of 8",
                    "Pull-ups - 3 sets to failure",
                    "Barbell Rows - 3 sets of 10"
                });
                break;

            case nameof(WorkoutRoutine.HighIntensityWorkout):
                description = "High-intensity interval training.";
                tasks.AddRange(new[]
                {
                    "Burpees - 30 sec",
                    "Mountain Climbers - 30 sec",
                    "Jump Squats - 30 sec",
                    "Rest - 30 sec (Repeat 4x)"
                });
                break;

            case nameof(WorkoutRoutine.SportsFitness):
                description = "Enhances agility and athletic performance.";
                tasks.AddRange(new[]
                {
                    "Agility Ladder - 10 min",
                    "Sprint Drills - 5 rounds",
                    "Cone Drills - 10 min"
                });
                break;
        }

        WorkoutDescription.Text = description;

        foreach (var task in tasks)
        {
            TasksLayout.Children.Add(new Label
            {
                Text = "• " + task,
                FontSize = 16
            });
        }
    }
}