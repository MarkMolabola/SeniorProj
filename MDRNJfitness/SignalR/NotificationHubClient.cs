using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using MDRNJ.Shared.DTOs.Notification;
using MDRNJfitness.Helpers;
using Microsoft.AspNetCore.SignalR.Client;

namespace MDRNJfitness.SignalR
{
    /// <summary>
    /// Live notifications (friend requests, duel invites and results): shows a toast and
    /// broadcasts a <see cref="NotificationMessage"/> so open pages can refresh.
    /// Connected from Home after sign-in; stopped on logout so it never outlives the session.
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
