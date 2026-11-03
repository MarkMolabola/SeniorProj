using MDRNJ.Shared.DTOs.Friend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IPresenceService
    {
        /// <summary>Status lights for these users, keyed by user ID (every ID gets an entry).</summary>
        Task<Dictionary<Guid, PresenceDto>> GetAsync(IReadOnlyCollection<Guid> userIds);

        /// <summary>
        /// Pushes the user's current status to their online friends ("PresenceChanged").
        /// Best effort: never throws, so a failed push can't fail the request.
        /// </summary>
        Task BroadcastAsync(Guid userId);
    }
}
