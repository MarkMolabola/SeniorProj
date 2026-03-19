using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class ChallengePage : ContentPage
{
    public ChallengePage(ChallengeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ChallengeViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
