using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

/// <summary>Pop-up hero picker opened by CUSTOMIZE on the Hero tab.</summary>
public partial class AvatarPickerPage : ContentPage
{
    public AvatarPickerPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is CharacterViewModel vm)
            vm.ClosePopupCommand.Execute(null);
        return true;
    }
}
