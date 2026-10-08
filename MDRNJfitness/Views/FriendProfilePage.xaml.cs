using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class FriendProfilePage : ContentPage
{
    public FriendProfilePage(FriendProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
