using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class QuestPage : ContentPage
{
    public QuestPage(QuestViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is QuestViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    private async void OnAdminPanelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminQuestPage));
    }
}
