using MDRNJfitness.Helpers;
using Microsoft.AspNetCore.SignalR.Client;

namespace MDRNJfitness.SignalR
{
    public class LeaderboardHubClient : IAsyncDisposable
    {
        private HubConnection? _connection;

        public event Action? LeaderboardUpdated;

        public async Task ConnectAsync()
        {
            // Already connected — nothing to do
            if (_connection?.State == HubConnectionState.Connected)
                return;

            // Dispose any previous stale connection
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            var token = await SecureStorage.GetAsync("access_token");

            _connection = new HubConnectionBuilder()
                .WithUrl($"{ApiConstants.BaseUrl}hubs/leaderboard?access_token={token}")
                .WithAutomaticReconnect()
                .Build();

            _connection.On("LeaderboardUpdated", () => LeaderboardUpdated?.Invoke());

            await _connection.StartAsync();
            await _connection.InvokeAsync("JoinLeaderboard");
        }

        public async Task DisconnectAsync()
        {
            if (_connection is null) return;
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                    await _connection.InvokeAsync("LeaveLeaderboard");
                await _connection.StopAsync();
            }
            catch
            {
                // Ignore disconnect errors — connection may already be gone
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
