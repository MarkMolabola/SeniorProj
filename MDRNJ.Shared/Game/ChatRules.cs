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
    }
}
