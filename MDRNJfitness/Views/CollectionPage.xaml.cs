using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class CollectionPage : ContentPage
{
    public CollectionPage(CollectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CollectionViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
