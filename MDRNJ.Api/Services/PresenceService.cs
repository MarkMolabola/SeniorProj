using MDRNJ.Api.Data;
using MDRNJ.Api.Hubs;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Friend status lights. Working out = a live session that hasn't ended (and isn't
    /// older than the reward cap, so an abandoned one stops counting); online = the app
    /// is connected to the NotificationHub; otherwise offline.
    /// </summary>
    public class PresenceService : IPresenceService
    {
        private readonly AppDbContext _db;
        private readonly PresenceTracker _tracker;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<PresenceService> _logger;

        public PresenceService(AppDbContext db, PresenceTracker tracker, IHubContext<NotificationHub> hub,
            ILogger<PresenceService> logger)
        {
            _db = db;
            _tracker = tracker;
            _hub = hub;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, PresenceDto>> GetAsync(IReadOnlyCollection<Guid> userIds)
        {
            var since = DateTime.UtcNow.AddMinutes(-WorkoutTypes.MaxDurationMins);
            var open = await _db.WorkoutSessions
                .Where(s => userIds.Contains(s.UserId) && s.EndTime == null && s.StartTime > since)
                .Select(s => new { s.UserId, s.StartTime, s.WorkoutType })
                .ToListAsync();
            var training = open
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.MaxBy(s => s.StartTime)!.WorkoutType);

            return userIds.Distinct().ToDictionary(id => id, id => new PresenceDto
            {
                UserId = id,
                Presence = (training.ContainsKey(id) ? PresenceStatus.WorkingOut
                    : _tracker.IsOnline(id) ? PresenceStatus.Online
                    : PresenceStatus.Offline).ToString(),
                WorkoutType = training.TryGetValue(id, out var type) ? type.ToString() : null
            });
        }

        public async Task BroadcastAsync(Guid userId)
        {
            try
            {
                var friendIds = await _db.Friendships
                    .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                                && f.Status == FriendshipStatus.Accepted)
                    .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                    .ToListAsync();
                var listening = friendIds.Where(_tracker.IsOnline).Select(id => id.ToString()).ToList();
                if (listening.Count == 0) return;

                var presence = (await GetAsync(new[] { userId }))[userId];
                await _hub.Clients.Users(listening).SendAsync("PresenceChanged", presence);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Couldn't push presence for {UserId}", userId);
            }
        }
    }
}
