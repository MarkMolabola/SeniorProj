using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
            await vm.TryResumeSessionAsync();
    }
}
