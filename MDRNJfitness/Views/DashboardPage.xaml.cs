using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    private async void OnStartWorkoutClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//WorkoutPage");
    }
}
