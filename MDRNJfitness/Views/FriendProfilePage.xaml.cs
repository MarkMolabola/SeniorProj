using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class FriendProfilePage : ContentPage
{
    public FriendProfilePage(FriendProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
