using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

/// <summary>Pop-up opened by tapping a gear slot on the Hero tab.</summary>
public partial class GearPickerPage : ContentPage
{
    public GearPickerPage(CharacterViewModel vm)
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
