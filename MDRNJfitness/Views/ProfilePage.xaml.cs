using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
