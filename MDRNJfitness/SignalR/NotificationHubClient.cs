using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Notification;
using MDRNJfitness.Helpers;
using Microsoft.AspNetCore.SignalR.Client;

namespace MDRNJfitness.SignalR
{
    /// <summary>
    /// Live notifications (friend requests, duel invites and results): shows a toast and
    /// broadcasts a <see cref="NotificationMessage"/> so open pages can refresh. Friends'
    /// status lights arrive here too, as a <see cref="PresenceChangedMessage"/>.
    /// Being connected is what shows this user as online, so it's connected from Home after
    /// sign-in and on resume, and stopped on logout and when the app goes to the background.
    /// </summary>
    public class NotificationHubClient : IAsyncDisposable
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private HubConnection? _connection;

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
                    .WithUrl($"{ApiConstants.BaseUrl}hubs/notifications", o => o.AccessTokenProvider = TokenStore.GetAccessTokenAsync)
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<NotificationDto>("Notify", OnNotified);
                _connection.On<PresenceDto>("PresenceChanged", presence =>
                    MainThread.BeginInvokeOnMainThread(() =>
                        WeakReferenceMessenger.Default.Send(new PresenceChangedMessage(presence))));
                await _connection.StartAsync();
            }
            catch
            {
                // Offline or server asleep: notifications are a bonus, Home tries again next time
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StopAsync()
        {
            await _gate.WaitAsync();
            try
            {
                if (_connection is null) return;
                await _connection.DisposeAsync();
                _connection = null;
            }
            finally
            {
                _gate.Release();
            }
        }

        private static void OnNotified(NotificationDto notification) =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(notification));
                await Toast.Make(notification.Message, ToastDuration.Long).Show();
            });

        public ValueTask DisposeAsync() => new(StopAsync());
    }
}
