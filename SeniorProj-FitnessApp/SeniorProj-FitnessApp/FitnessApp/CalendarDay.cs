using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace FitnessApp;

public class CalendarDay : INotifyPropertyChanged
{
    public DateTime? Date { get; set; } // Nullable for placeholder days

    public bool IsPlaceholder => !Date.HasValue;

    public int DayNumber => Date?.Day ?? 0;

    private bool isExercised;
    public bool IsExercised
    {
        get => isExercised;
        set
        {
            if (isExercised != value)
            {
                isExercised = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
    }

    private int steps;
    public int Steps
    {
        get => steps;
        set
        {
            if (steps != value)
            {
                steps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StepsBarHeight));
                OnPropertyChanged(nameof(StepsDisplay));
            }
        }
    }

    public TimeSpan ExerciseTime { get; set; } = TimeSpan.Zero;

    public Color BackgroundColor =>
        IsPlaceholder ? Colors.Transparent : IsExercised ? Colors.Green : Colors.White;

    private const int MaxSteps = 10000;

    public double StepsBarHeight
    {
        get
        {
            double normalized = (double)Steps / MaxSteps;
            if (normalized > 1) normalized = 1;
            if (normalized < 0) normalized = 0;
            return normalized;
        }
    }

    public string StepsDisplay =>
        Steps > 0 ? $"{Steps}/{MaxSteps} steps taken." : string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
