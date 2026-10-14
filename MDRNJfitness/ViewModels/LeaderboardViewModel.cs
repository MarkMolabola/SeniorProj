using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Leaderboard;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class LeaderboardViewModel : BaseViewModel
    {
        private readonly ILeaderboardService _leaderboard;
        private readonly LeaderboardHubClient _hub;
        private readonly IAuthService _auth;

        public ObservableCollection<LeaderboardEntryDto> Entries { get; } = new();

        public LeaderboardViewModel(ILeaderboardService leaderboard, LeaderboardHubClient hub, IAuthService auth)
        {
            _leaderboard = leaderboard;
            _hub = hub;
            _auth = auth;
            _hub.LeaderboardUpdated += OnLeaderboardUpdated;
            Title = "Leaderboard";
        }

        private void OnLeaderboardUpdated() =>
            MainThread.BeginInvokeOnMainThread(() => LoadCommand.Execute(null));

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var entries = await _leaderboard.GetTopAsync(50);
                Entries.Clear();
                foreach (var e in entries) Entries.Add(e);
            });

        /// <summary>Opens another player's profile (tapping your own row does nothing).</summary>
        [RelayCommand]
        public async Task OpenProfileAsync(LeaderboardEntryDto entry)
        {
            var me = await _auth.GetCurrentUserAsync();
            if (me is not null && me.UserId == entry.UserId) return;
            await Shell.Current.GoToAsync($"FriendProfilePage?userId={entry.UserId}");
        }

        public Task ConnectAsync() => _hub.ConnectAsync();
        public Task DisconnectAsync() => _hub.DisconnectAsync();
    }
}
