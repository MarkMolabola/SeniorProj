using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class WorkoutPage : ContentPage
{
    public WorkoutPage(WorkoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is WorkoutViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
