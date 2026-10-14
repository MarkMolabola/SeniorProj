using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class AdminReportsPage : ContentPage
{
    private readonly AdminReportsViewModel _vm;

    public AdminReportsPage(AdminReportsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
