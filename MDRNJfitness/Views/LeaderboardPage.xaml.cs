using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardViewModel _vm;

    public LeaderboardPage(LeaderboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.ConnectAsync(); } catch { /* real-time updates unavailable */ }
        _vm.LoadCommand.Execute(null);
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        try { await _vm.DisconnectAsync(); } catch { }
    }
}
