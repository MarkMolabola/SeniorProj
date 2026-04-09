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

        public ObservableCollection<LeaderboardEntryDto> Entries { get; } = new();

        public LeaderboardViewModel(ILeaderboardService leaderboard, LeaderboardHubClient hub)
        {
            _leaderboard = leaderboard;
            _hub = hub;
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

        public Task ConnectAsync() => _hub.ConnectAsync();
        public Task DisconnectAsync() => _hub.DisconnectAsync();
    }
}
