using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class ChatRoomPage : ContentPage
{
    private readonly ChatRoomViewModel _vm;

    public ChatRoomPage(ChatRoomViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.ScrollToEndRequested += ScrollToEnd;

        // New messages keep the newest in view; older ones inserted at the top shouldn't jump the list
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ChatRoomViewModel.IsLoadingOlder))
                MessageList.ItemsUpdatingScrollMode = _vm.IsLoadingOlder
                    ? ItemsUpdatingScrollMode.KeepScrollOffset
                    : ItemsUpdatingScrollMode.KeepLastItemInView;
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.AppearingAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _vm.DisappearingAsync();
    }

    private void ScrollToEnd()
    {
        // A new bubble gets its real height a moment after it's added, so a single scroll
        // stops one message short; scroll again once layout has settled.
        foreach (var delay in new[] { 80, 350 })
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delay), () =>
            {
                if (_vm.Messages.Count > 0)
                    MessageList.ScrollTo(_vm.Messages[^1], position: ScrollToPosition.End, animate: false);
            });
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
