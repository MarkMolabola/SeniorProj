namespace MDRNJ.Shared.Game
{
    /// <summary>The Terms of Service and Privacy Policy, served by the API at /terms and /privacy.</summary>
    public static class LegalRules
    {
        /// <summary>
        /// Raise this when the Terms or Privacy Policy change meaningfully: every player is
        /// asked to accept again on their next sign-in.
        /// </summary>
        public const int TermsVersion = 1;
        public const string EffectiveDate = "October 18, 2026";

        public const int MinimumAge = 13;

        public const string TermsPath = "terms";
        public const string PrivacyPath = "privacy";
    }
}
