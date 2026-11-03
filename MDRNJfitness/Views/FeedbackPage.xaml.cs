using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class FeedbackPage : ContentPage
{
    public FeedbackPage(FeedbackViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
