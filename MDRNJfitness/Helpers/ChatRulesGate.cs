using MDRNJ.Shared.Game;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Players agree to the chat rules once (per ChatRules.RulesVersion) before posting,
    /// as app stores require for user-generated content.
    /// </summary>
    public static class ChatRulesGate
    {
        private const string Key = "chat_rules_accepted_version";

        public static async Task<bool> EnsureAcceptedAsync()
        {
            if (Preferences.Default.Get(Key, 0) >= ChatRules.RulesVersion)
                return true;

            var accepted = await Shell.Current.DisplayAlertAsync("TAVERN RULES", ChatRules.RulesText, "I AGREE", "Not now");
            if (accepted)
                Preferences.Default.Set(Key, ChatRules.RulesVersion);
            return accepted;
        }
    }
}
