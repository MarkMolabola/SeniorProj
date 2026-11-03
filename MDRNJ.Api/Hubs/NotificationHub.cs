using MDRNJ.Api.Services;
using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Hubs
{
    /// <summary>
    /// Live notifications (friend requests, duel invites and results). Clients only
    /// listen: the server pushes "Notify" (NotificationDto) to a user via INotifier,
    /// and "PresenceChanged" (PresenceDto) when a friend's status light changes.
    /// Being connected here is what makes a user show as online to their friends.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly PresenceTracker _tracker;
        private readonly IPresenceService _presence;

        public NotificationHub(PresenceTracker tracker, IPresenceService presence)
        {
            _tracker = tracker;
            _presence = presence;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            if (Guid.TryParse(Context.UserIdentifier, out var userId) && _tracker.Connected(userId))
                await _presence.BroadcastAsync(userId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Guid.TryParse(Context.UserIdentifier, out var userId) && _tracker.Disconnected(userId))
                await _presence.BroadcastAsync(userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
