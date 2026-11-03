namespace MDRNJ.Shared.Game
{
    /// <summary>Limits on Profile ▸ SEND FEEDBACK (checked by the app and the API).</summary>
    public static class FeedbackRules
    {
        public const int MinLength = 10;
        public const int MaxLength = 2000;
        /// <summary>Per player per rolling 24 hours, so one tester can't flood the inbox.</summary>
        public const int MaxPerDay = 10;
    }
}
