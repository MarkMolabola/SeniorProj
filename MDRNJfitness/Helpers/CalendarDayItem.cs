using CommunityToolkit.Mvvm.ComponentModel;

namespace MDRNJfitness.Helpers
{
    public partial class CalendarDayItem : ObservableObject
    {
        public DateTime Date { get; init; }
        public int Day => Date.Day;
        public bool IsCurrentMonth { get; init; }
        public bool IsToday => Date.Date == DateTime.Today;

        [ObservableProperty] private bool _hasWorkout;
        [ObservableProperty] private bool _isSelected;
    }
}
