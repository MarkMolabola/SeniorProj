using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Feedback;
using MDRNJ.Shared.Enums;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>One row in the admin feedback inbox.</summary>
    public class FeedbackCard
    {
        public FeedbackCard(FeedbackDto dto) => Feedback = dto;

        public FeedbackDto Feedback { get; }
        public bool IsBug => Feedback.Kind == FeedbackKind.Bug;
        public string KindText => IsBug ? "BUG" : "IDEA";
        public string Header => $"{Feedback.Username} · {Feedback.CreatedAt.ToLocalTime():MMM d, h:mm tt}";
        public string DeviceText => string.Join(" · ", new[] { Feedback.AppVersion, Feedback.Platform, Feedback.DeviceModel }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        public bool IsResolved => Feedback.IsResolved;
        public string ActionText => IsResolved ? "REOPEN" : "RESOLVE";
    }

    /// <summary>Admins: read bug reports and suggestions, mark them resolved.</summary>
    public partial class AdminFeedbackViewModel : BaseViewModel
    {
        private readonly IFeedbackService _feedback;

        public ObservableCollection<FeedbackCard> Items { get; } = new();
        [ObservableProperty] private bool _isEmpty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToggleResolvedText))]
        private bool _showResolved;

        public string ToggleResolvedText => ShowResolved ? "HIDE RESOLVED" : "SHOW RESOLVED";

        public AdminFeedbackViewModel(IFeedbackService feedback)
        {
            _feedback = feedback;
            Title = "Feedback inbox";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var items = await _feedback.GetInboxAsync(ShowResolved);
                Items.Clear();
                foreach (var f in items) Items.Add(new FeedbackCard(f));
                IsEmpty = Items.Count == 0;
            });

        [RelayCommand]
        public async Task ToggleResolvedAsync()
        {
            ShowResolved = !ShowResolved;
            await LoadAsync();
        }

        [RelayCommand]
        public async Task ToggleItemAsync(FeedbackCard card)
        {
            await RunSafeAsync(async () =>
            {
                if (card.IsResolved) await _feedback.ReopenAsync(card.Feedback.FeedbackId);
                else await _feedback.ResolveAsync(card.Feedback.FeedbackId);
            });
            await LoadAsync();
        }

        [RelayCommand]
        public Task OpenSenderAsync(FeedbackCard card) =>
            Shell.Current.GoToAsync($"FriendProfilePage?userId={card.Feedback.UserId}");
    }
}
