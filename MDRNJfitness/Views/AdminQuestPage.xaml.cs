using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class AdminQuestPage : ContentPage
{
    public AdminQuestPage(AdminQuestViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminQuestViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
