using System.Diagnostics;

namespace MDRNJfitness.Views;

public partial class DuelStopwatchPage : ContentPage
{
    private readonly TaskCompletionSource<float?> _result = new();
    private readonly Stopwatch _stopwatch = new();
    private IDispatcherTimer? _timer;

    public DuelStopwatchPage(string title, string hint)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        HintLabel.Text = hint;
    }

    /// <summary>Seconds on the clock when submitted, or null if cancelled.</summary>
    public Task<float?> Result => _result.Task;

    private void OnStartStopClicked(object? sender, EventArgs e)
    {
        if (_stopwatch.IsRunning)
        {
            Stop();
            return;
        }

        _stopwatch.Restart();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += (_, _) => ShowTime();
        _timer.Start();

        StartStopButton.Text = "STOP";
        RetryButton.IsEnabled = SubmitButton.IsEnabled = false;
    }

    private void Stop()
    {
        _stopwatch.Stop();
        _timer?.Stop();
        ShowTime();

        StartStopButton.Text = "START";
        StartStopButton.IsEnabled = false;
        RetryButton.IsEnabled = true;
        SubmitButton.IsEnabled = _stopwatch.Elapsed.TotalSeconds >= 1;
    }

    private void ShowTime()
    {
        var t = _stopwatch.Elapsed;
        TimeLabel.Text = t.TotalHours >= 1
            ? t.ToString(@"h\:mm\:ss")
            : $"{(int)t.TotalMinutes}:{t.Seconds:D2}.{t.Milliseconds / 100}";
    }

    private void OnRetryClicked(object? sender, EventArgs e)
    {
        _stopwatch.Reset();
        ShowTime();
        StartStopButton.IsEnabled = true;
        RetryButton.IsEnabled = SubmitButton.IsEnabled = false;
    }

    private async void OnSubmitClicked(object? sender, EventArgs e) =>
        await CloseAsync((float)Math.Round(_stopwatch.Elapsed.TotalSeconds, 1));

    private async void OnCancelClicked(object? sender, EventArgs e) => await CloseAsync(null);

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync(null);
        return true;
    }

    private async Task CloseAsync(float? seconds)
    {
        _timer?.Stop();
        if (!_result.TrySetResult(seconds)) return;
        await Navigation.PopModalAsync();
    }
}
