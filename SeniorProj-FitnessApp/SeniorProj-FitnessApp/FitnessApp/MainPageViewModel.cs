using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Linq;

namespace FitnessApp;

public class MainPageViewModel : INotifyPropertyChanged
{
    private const int TenDayTotalThreshold = 10;
    private const int TwentyDayTotalThreshold = 20;
    private const int FiveDayStreakThreshold = 5;

    public ObservableCollection<CalendarDay> CalendarDays { get; set; }

    public ICommand ToggleDayCommand { get; }

    public string CurrentMonth => DateTime.Now.ToString("MMMM yyyy");

    private int exercisedDaysCount;
    public int ExercisedDaysCount
    {
        get => exercisedDaysCount;
        set
        {
            if (exercisedDaysCount != value)
            {
                exercisedDaysCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasTenDayTotalBadge));
                OnPropertyChanged(nameof(HasTwentyDayTotalBadge));
            }
        }
    }

    private bool hasFiveDayStreakBadge;
    public bool HasFiveDayStreakBadge
    {
        get => hasFiveDayStreakBadge;
        set
        {
            if (hasFiveDayStreakBadge != value)
            {
                hasFiveDayStreakBadge = value;
                OnPropertyChanged();
            }
        }
    }

    // New flag to track if 5-day streak alert has been shown
    private bool fiveDayStreakAlertShown = false;

    public bool HasTenDayTotalBadge => ExercisedDaysCount >= TenDayTotalThreshold;
    public bool HasTwentyDayTotalBadge => ExercisedDaysCount >= TwentyDayTotalThreshold;

    public MainPageViewModel()
    {
        CalendarDays = new ObservableCollection<CalendarDay>();
        ToggleDayCommand = new Command<CalendarDay>(async (day) => await ToggleDay(day));

        GenerateCalendar();
    }

    private void GenerateCalendar()
    {
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        int offset = (int)firstDayOfMonth.DayOfWeek;

        for (int i = 0; i < offset; i++)
            CalendarDays.Add(new CalendarDay { Date = null });

        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        for (int i = 1; i <= daysInMonth; i++)
        {
            CalendarDays.Add(new CalendarDay
            {
                Date = new DateTime(today.Year, today.Month, i)
            });
        }
    }

    private async Task ToggleDay(CalendarDay day)
    {
        if (day == null || day.IsPlaceholder)
            return;

        var steps = await StepsInputService.PromptForSteps(day.DayNumber, day.Steps);

        if (steps.HasValue)
        {
            bool wasExercisedBefore = day.IsExercised;

            day.Steps = steps.Value;
            day.IsExercised = steps.Value > 0;

            if (!wasExercisedBefore && day.IsExercised)
                ExercisedDaysCount++;
            else if (wasExercisedBefore && !day.IsExercised)
                ExercisedDaysCount--;

            CheckFiveDayStreak();
            await CheckBadges();
        }
    }

    private void CheckFiveDayStreak()
    {
        var exercisedDates = CalendarDays
            .Where(d => d.Date.HasValue && d.IsExercised)
            .Select(d => d.Date.Value)
            .OrderBy(d => d)
            .ToList();

        if (!exercisedDates.Any())
        {
            HasFiveDayStreakBadge = false;
            return;
        }

        int currentStreak = 1;
        int maxStreak = 1;

        for (int i = 1; i < exercisedDates.Count; i++)
        {
            if ((exercisedDates[i] - exercisedDates[i - 1]).Days == 1)
            {
                currentStreak++;
                maxStreak = System.Math.Max(maxStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }

        bool earnedStreakBadge = maxStreak >= FiveDayStreakThreshold;
        HasFiveDayStreakBadge = earnedStreakBadge;
    }

    private async Task CheckBadges()
    {
        if (ExercisedDaysCount == TenDayTotalThreshold)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Achievement Unlocked!",
                "You earned the 10-Day Fitness Badge! 🥈",
                "Awesome!");
        }

        // Only show 5-day streak alert once
        if (HasFiveDayStreakBadge && !fiveDayStreakAlertShown)
        {
            fiveDayStreakAlertShown = true;
            await Application.Current.MainPage.DisplayAlert(
                "Streak Master!",
                "You earned the 5-Day Consecutive Streak Badge! 🥉",
                "Keep it going!");
        }

        if (ExercisedDaysCount == TwentyDayTotalThreshold)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Legendary Achievement!",
                "You earned the 20-Day Fitness Badge! 🥇",
                "Incredible!");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}