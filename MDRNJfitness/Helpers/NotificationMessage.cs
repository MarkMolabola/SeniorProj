using MDRNJ.Shared.DTOs.Notification;

namespace MDRNJfitness.Helpers;

/// <summary>A live notification arrived (friend request, duel invite / result, ...).</summary>
public record NotificationMessage(NotificationDto Notification)
{
    public bool IsAboutChallenges => Notification.Type.StartsWith("Challenge", StringComparison.Ordinal);
    public bool IsAboutFriends => Notification.Type.StartsWith("Friend", StringComparison.Ordinal);
}
