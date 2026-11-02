using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class WorkoutPage : ContentPage
{
    public WorkoutPage(WorkoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private bool _pressing;

    private async void OnCircleTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not WorkoutViewModel vm || vm.IsBusy || _pressing) return;
        _pressing = true; // a double tap would start twice or stack two finish prompts
        try
        {
            // A quick squash so the press feels physical
            await Circle.ScaleToAsync(0.93, 70, Easing.CubicOut);
            await Circle.ScaleToAsync(1.0, 110, Easing.CubicIn);
            await vm.PrimaryActionCommand.ExecuteAsync(null);
        }
        finally
        {
            _pressing = false;
        }
    }
}
