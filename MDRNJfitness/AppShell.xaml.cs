using SelectWorkoutRoutine.Views;

namespace SelectWorkoutRoutine;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register route for detail page
        Routing.RegisterRoute(nameof(WorkoutDetailPage), typeof(WorkoutDetailPage));
    }
}
