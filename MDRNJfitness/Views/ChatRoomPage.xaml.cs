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
        if (_vm.Messages.Count > 0)
            MessageList.ScrollTo(_vm.Messages.Count - 1, position: ScrollToPosition.End, animate: false);
    }

    // The pixel theme hides the nav bar, so pushed pages provide their own back button
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
