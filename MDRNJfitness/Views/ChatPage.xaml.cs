using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatListViewModel _vm;

    public ChatPage(ChatListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.AppearingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Disappearing();
    }
}
