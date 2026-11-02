using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class HistoryPage : ContentPage
{
    public HistoryPage(HistoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistoryViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
