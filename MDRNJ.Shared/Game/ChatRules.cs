namespace MDRNJ.Shared.Game
{
    /// <summary>Chat limits, shared so the API (validation) and the app (input limits) agree.</summary>
    public static class ChatRules
    {
        /// <summary>The single global room everyone can read and post in.</summary>
        public static readonly Guid GlobalChannelId = new("6b1f7c0e-8f2a-4c1e-9d3b-1a2b3c4d5e6f");

        public const int MaxMessageLength = 500;
        public const int MaxGroupNameLength = 40;
        public const int MaxGroupMembers = 50;

        /// <summary>Messages fetched per page when opening or scrolling back in a chat.</summary>
        public const int PageSize = 50;

        /// <summary>Spam limit: at most this many messages per user within RateLimitWindow.</summary>
        public const int RateLimitCount = 5;
        public static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(5);

        /// <summary>A message is hidden for everyone once this many players report it (until an admin reviews it).</summary>
        public const int ReportsToHide = 3;

        /// <summary>Shown to a player an admin has banned when they try to sign in or chat.</summary>
        public const string SuspendedMessage = "This account has been suspended for breaking the community rules.";

        /// <summary>Reasons offered when reporting a message.</summary>
        public static readonly IReadOnlyList<string> ReportReasons = new[]
        {
            "Spam", "Harassment or hate", "Sexual or violent content", "Other"
        };

        /// <summary>Bump to show the rules again after they change.</summary>
        public const int RulesVersion = 2;

        public const string RulesText =
            "Zero tolerance: no harassment, hate speech, threats, sexual content, spam or cheating. " +
            "Tap any message to report it or block its sender. Moderators review reports and remove " +
            "content, and players who break the rules are muted or banned. " +
            "Full rules: Terms of Service (Profile tab).";
    }
}
