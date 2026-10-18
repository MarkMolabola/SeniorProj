using MDRNJ.Shared.DTOs.Chat;

namespace MDRNJfitness.Models.Social
{
    /// <summary>A row in the chat list.</summary>
    public class ChatChannelCard
    {
        public ChatChannelCard(ChatChannelDto channel) => Channel = channel;

        public ChatChannelDto Channel { get; }

        public string Name => Channel.Name;

        public string KindLabel => Channel.Kind switch
        {
            "Global" => "GLOBAL",
            "Group" => $"GROUP · {Channel.MemberCount}",
            _ => "DIRECT"
        };

        public string Preview => Channel.LastMessagePreview is null
            ? "No messages yet. Say hi!"
            : Channel.Kind == "Direct" ? Channel.LastMessagePreview : $"{Channel.LastSenderName}: {Channel.LastMessagePreview}";

        public string TimeText => Channel.LastMessageAt is DateTime t ? ChatTime.Short(t) : string.Empty;

        public bool HasUnread => Channel.UnreadCount > 0;
        public string UnreadText => Channel.UnreadCount > 99 ? "99+" : Channel.UnreadCount.ToString();
    }

    /// <summary>A message bubble: mine on the right, everyone else's on the left.</summary>
    public class ChatMessageItem
    {
        public ChatMessageItem(ChatMessageDto message, bool isMine, bool showSender)
        {
            Message = message;
            IsMine = isMine;
            ShowSender = showSender && !isMine;
        }

        public ChatMessageDto Message { get; }
        public bool IsMine { get; }

        /// <summary>Names are shown in global and group chats (not direct chats or on my own messages).</summary>
        public bool ShowSender { get; }

        public string SenderName => Message.SenderName;
        public string Body => Message.Body;
        public string TimeText => ChatTime.Short(Message.SentAt);
        public LayoutOptions Alignment => IsMine ? LayoutOptions.End : LayoutOptions.Start;
    }

    /// <summary>A reported message in the admin queue.</summary>
    public class ReportedMessageCard
    {
        public ReportedMessageCard(ReportedMessageDto report) => Report = report;

        public ReportedMessageDto Report { get; }
        public string Header => $"{Report.SenderName} · {Report.ChannelName} · {ChatTime.Short(Report.SentAt)}";
        public string Body => Report.Body;
        public string ReportText => $"{Report.ReportCount} report{(Report.ReportCount == 1 ? "" : "s")}: {string.Join(", ", Report.Reasons)}"
            + (Report.IsHidden ? " · HIDDEN" : "");

        /// <summary>What's already been done to the sender, if anything.</summary>
        public string SenderStatusText => Report.SenderBanned ? $"{Report.SenderName} is BANNED"
            : Report.SenderMutedUntil is DateTime until ? $"{Report.SenderName} is muted until {ChatTime.Short(until)}"
            : string.Empty;
        public bool HasSenderStatus => SenderStatusText.Length > 0;
    }

    public static class ChatTime
    {
        /// <summary>"3:42 PM" today, "Mon" this week, else "Oct 3". API times are UTC.</summary>
        public static string Short(DateTime sentAt)
        {
            var utc = sentAt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(sentAt, DateTimeKind.Utc) : sentAt;
            var local = utc.ToLocalTime();
            var today = DateTime.Now.Date;
            if (local.Date == today) return local.ToString("h:mm tt");
            if (local.Date > today.AddDays(-7)) return local.ToString("ddd");
            return local.ToString("MMM d");
        }
    }
}
