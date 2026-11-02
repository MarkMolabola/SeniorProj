using System;
using System.Collections.Concurrent;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Who has the app open: counts each user's live NotificationHub connections
    /// (a phone and an emulator are two). In memory, so it's per server instance,
    /// which is fine while the API runs on a single App Service instance.
    /// </summary>
    public class PresenceTracker
    {
        private readonly ConcurrentDictionary<Guid, int> _connections = new();

        /// <summary>Returns true when this was the user's first connection (they just came online).</summary>
        public bool Connected(Guid userId) =>
            _connections.AddOrUpdate(userId, 1, (_, count) => count + 1) == 1;

        /// <summary>Returns true when this was the user's last connection (they just went offline).</summary>
        public bool Disconnected(Guid userId)
        {
            while (_connections.TryGetValue(userId, out var count))
            {
                if (count <= 1)
                {
                    if (_connections.TryRemove(new(userId, count))) return true;
                }
                else if (_connections.TryUpdate(userId, count - 1, count))
                {
                    return false;
                }
            }
            return false;
        }

        public bool IsOnline(Guid userId) => _connections.ContainsKey(userId);
    }
}
