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

    /// <summary>SHOW / HIDE beside a password field: reveals the Entry that shares its row.</summary>
    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Parent is not Grid row) return;
        var entry = row.Children.OfType<Entry>().FirstOrDefault();
        if (entry is null) return;

        entry.IsPassword = !entry.IsPassword;
        button.Text = entry.IsPassword ? "SHOW" : "HIDE";
    }
}
