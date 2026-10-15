using CommunityToolkit.Mvvm.Messaging;
using MDRNJfitness.Helpers;
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

        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) =>
        {
            if (m.IsAboutChallenges && BindingContext is ChallengeViewModel open)
                open.LoadCommand.Execute(null);
        });
    }

    // Live notifications: refresh when one about challenges arrives while this page is open
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<NotificationMessage>(this);
    }
}
