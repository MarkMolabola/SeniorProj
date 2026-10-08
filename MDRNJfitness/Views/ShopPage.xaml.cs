using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class ShopPage : ContentPage
{
    public ShopPage(ShopViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ShopViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
