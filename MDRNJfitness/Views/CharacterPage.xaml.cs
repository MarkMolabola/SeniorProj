using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class CharacterPage : ContentPage
{
    public CharacterPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CharacterViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
