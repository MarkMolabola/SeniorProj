using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Feedback;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.ViewModels
{
    /// <summary>
    /// Profile ▸ SEND FEEDBACK: a bug report or an idea. App version and device are attached
    /// automatically so testers don't have to type them.
    /// </summary>
    public partial class FeedbackViewModel : BaseViewModel
    {
        private readonly IFeedbackService _feedback;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBug), nameof(IsSuggestion), nameof(Prompt), nameof(Placeholder))]
        private FeedbackKind _kind = FeedbackKind.Bug;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CountText))]
        private string _message = string.Empty;

        public bool IsBug => Kind == FeedbackKind.Bug;
        public bool IsSuggestion => Kind == FeedbackKind.Suggestion;

        public string Prompt => IsBug
            ? "What went wrong? What were you doing right before it happened?"
            : "What would make the game better?";

        public string Placeholder => IsBug
            ? "e.g. I tapped Continue after a duel and the screen froze."
            : "e.g. Let me rename my hero.";

        public string CountText => $"{Message.Trim().Length} / {FeedbackRules.MaxLength}";
        public int MaxLength => FeedbackRules.MaxLength;

        /// <summary>Shown under the form so testers know what's sent along.</summary>
        public string DeviceText { get; } =
            $"Also sent: app {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString}) · {DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString} · {DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}";

        public FeedbackViewModel(IFeedbackService feedback)
        {
            _feedback = feedback;
            Title = "Feedback";
        }

        [RelayCommand]
        public void PickBug() => Kind = FeedbackKind.Bug;

        [RelayCommand]
        public void PickSuggestion() => Kind = FeedbackKind.Suggestion;

        [RelayCommand]
        public async Task SendAsync()
        {
            var text = Message.Trim();
            if (text.Length < FeedbackRules.MinLength)
            {
                await Shell.Current.DisplayAlertAsync("Feedback",
                    $"Tell us a bit more (at least {FeedbackRules.MinLength} characters).", "OK");
                return;
            }

            await RunSafeAsync(async () =>
            {
                await _feedback.SubmitAsync(new SubmitFeedbackDto
                {
                    Kind = Kind,
                    Message = text,
                    AppVersion = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})",
                    Platform = $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}",
                    DeviceModel = $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}",
                });

                Message = string.Empty;
                await Shell.Current.DisplayAlertAsync("Thanks!",
                    IsBug ? "Bug report sent. We'll squash it." : "Idea sent. Thanks for helping shape the game.", "OK");
                await Shell.Current.GoToAsync("..");
            });
        }
    }
}
