using MDRNJfitness.Helpers;
using MDRNJfitness.ViewModels;

namespace MDRNJfitness.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _vm;

    public CalendarPage(CalendarViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }

    private void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CalendarDayItem day)
        {
            _vm.SelectDayCommand.Execute(day);
            // Keep CollectionView in sync without firing this handler again
            DaysGrid.SelectedItem = null;
        }
    }
}
