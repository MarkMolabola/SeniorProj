using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class AdminQuestPage : ContentPage
{
    public AdminQuestPage(AdminQuestViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminQuestViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
