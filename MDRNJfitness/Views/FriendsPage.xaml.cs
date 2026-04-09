using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class FriendsPage : ContentPage
{
    public FriendsPage(FriendsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FriendsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
