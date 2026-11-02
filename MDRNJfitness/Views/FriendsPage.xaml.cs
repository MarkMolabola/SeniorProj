using CommunityToolkit.Mvvm.Messaging;
using MDRNJfitness.Helpers;
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

        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) =>
        {
            if (m.IsAboutFriends && BindingContext is FriendsViewModel open)
                open.LoadCommand.Execute(null);
        });
        WeakReferenceMessenger.Default.Register<PresenceChangedMessage>(this, (_, m) =>
            (BindingContext as FriendsViewModel)?.ApplyPresence(m.Presence));
    }

    // Live notifications: refresh when one about friends arrives while this page is open
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<NotificationMessage>(this);
        WeakReferenceMessenger.Default.Unregister<PresenceChangedMessage>(this);
    }
}
