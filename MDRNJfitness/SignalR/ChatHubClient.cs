using MDRNJ.Shared.DTOs.Chat;
using MDRNJfitness.Helpers;
using Microsoft.AspNetCore.SignalR.Client;

namespace MDRNJfitness.SignalR
{
    /// <summary>
    /// Live chat connection, shared by the chat list and chat rooms. Stays connected once
    /// started; connect after a REST call so the access token is fresh.
    /// </summary>
    public class ChatHubClient : IAsyncDisposable
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private HubConnection? _connection;
        private Guid? _joinedChannel;

        /// <summary>A new message in the chat currently joined.</summary>
        public event Action<ChatMessageDto>? MessageReceived;

        /// <summary>Something changed in one of the user's group/direct chats (refresh the list).</summary>
        public event Action<Guid>? ChannelActivity;

        public async Task ConnectAsync()
        {
            await _gate.WaitAsync();
            try
            {
                if (_connection?.State is HubConnectionState.Connected or HubConnectionState.Connecting or HubConnectionState.Reconnecting)
                    return;

                if (_connection is not null)
                    await _connection.DisposeAsync();

                _connection = new HubConnectionBuilder()
                    .WithUrl($"{ApiConstants.BaseUrl}hubs/chat", o => o.AccessTokenProvider = TokenStore.GetAccessTokenAsync)
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<ChatMessageDto>("MessageReceived", m => MessageReceived?.Invoke(m));
                _connection.On<Guid>("ChannelActivity", id => ChannelActivity?.Invoke(id));

                // Hub groups don't survive a reconnect: rejoin the open chat
                _connection.Reconnected += async _ =>
                {
                    if (_joinedChannel is Guid id)
                        await _connection.InvokeAsync("JoinChannel", id);
                };

                await _connection.StartAsync();
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task JoinChannelAsync(Guid channelId)
        {
            await ConnectAsync();
            await _connection!.InvokeAsync("JoinChannel", channelId);
            _joinedChannel = channelId;
        }

        public async Task LeaveChannelAsync(Guid channelId)
        {
            if (_joinedChannel == channelId)
                _joinedChannel = null;
            try
            {
                if (_connection?.State == HubConnectionState.Connected)
                    await _connection.InvokeAsync("LeaveChannel", channelId);
            }
            catch
            {
                // Connection already gone — nothing to leave
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
