using MDRNJ.Api.Hubs;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Notification;
using MDRNJ.Shared.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class Notifier : INotifier
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<Notifier> _logger;

        public Notifier(IHubContext<NotificationHub> hub, ILogger<Notifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task NotifyAsync(Guid userId, NotificationType type, string message, Guid? fromUserId = null, Guid? relatedId = null)
        {
            try
            {
                await _hub.Clients.User(userId.ToString()).SendAsync("Notify", new NotificationDto
                {
                    Type = type.ToString(),
                    Message = message,
                    FromUserId = fromUserId,
                    RelatedId = relatedId
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Couldn't push {Type} notification to {UserId}", type, userId);
            }
        }
    }
}
