using MDRNJ.Shared.Enums;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    /// <summary>Pushes live notifications to a player's connected app (if any).</summary>
    public interface INotifier
    {
        /// <summary>Best effort: never throws, so a failed push can't fail the request.</summary>
        Task NotifyAsync(Guid userId, NotificationType type, string message, Guid? fromUserId = null, Guid? relatedId = null);
    }
}
