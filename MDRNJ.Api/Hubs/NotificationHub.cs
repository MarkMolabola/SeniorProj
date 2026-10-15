using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MDRNJ.Api.Hubs
{
    /// <summary>
    /// Live notifications (friend requests, duel invites and results). Clients only
    /// listen: the server pushes "Notify" (NotificationDto) to a user via INotifier.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
