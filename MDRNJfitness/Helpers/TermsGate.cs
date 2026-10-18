using MDRNJ.Shared.Game;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// The Terms of Service and Privacy Policy: opening them, and making sure the signed-in
    /// player has accepted the current version (Google sign-ups, players from before the
    /// terms existed, and everyone after LegalRules.TermsVersion goes up).
    /// </summary>
    public static class TermsGate
    {
        private const string Agree = "I AGREE";
        private const string ReadTerms = "Read Terms of Service";
        private const string ReadPrivacy = "Read Privacy Policy";
        private const string SignOut = "Sign out";

        public static Task OpenTermsAsync() => OpenAsync(LegalRules.TermsPath);
        public static Task OpenPrivacyAsync() => OpenAsync(LegalRules.PrivacyPath);

        private static Task OpenAsync(string path) =>
            Browser.Default.OpenAsync(new Uri(new Uri(ApiConstants.BaseUrl), path), BrowserLaunchMode.SystemPreferred);

        /// <summary>True once the player has accepted; false if they chose to sign out instead.</summary>
        public static async Task<bool> EnsureAcceptedAsync(IAuthService auth)
        {
            var user = await auth.GetCurrentUserAsync();
            if (user is null || user.TermsVersion >= LegalRules.TermsVersion)
                return true;

            var prompt = user.TermsVersion == 0
                ? $"Before you play: you must be {LegalRules.MinimumAge} or older and agree to the Terms of Service and Privacy Policy. Our community has zero tolerance for abuse."
                : "We updated our Terms of Service and Privacy Policy. Please review and agree to keep playing.";

            while (true)
            {
                var choice = await Shell.Current.DisplayActionSheetAsync(prompt, null, null, Agree, ReadTerms, ReadPrivacy, SignOut);
                switch (choice)
                {
                    case Agree:
                        await auth.AcceptTermsAsync();
                        return true;
                    case ReadTerms:
                        await OpenTermsAsync();
                        break;
                    case ReadPrivacy:
                        await OpenPrivacyAsync();
                        break;
                    case SignOut:
                        await auth.LogoutAsync();
                        return false;
                    // Dismissed (back button / tap outside): ask again
                }
            }
        }
    }
}
