using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private IDispatcherTimer? _goldTimer;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not DashboardViewModel vm) return;
        vm.LoadCommand.Execute(null);

        // Dungeon gold ticks up live while Home is open
        _goldTimer = Dispatcher.CreateTimer();
        _goldTimer.Interval = TimeSpan.FromSeconds(1);
        _goldTimer.Tick += (_, _) => vm.UpdatePendingGold();
        _goldTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _goldTimer?.Stop();
        _goldTimer = null;
    }

    private async void OnStartWorkoutClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//WorkoutPage");
    }
}
