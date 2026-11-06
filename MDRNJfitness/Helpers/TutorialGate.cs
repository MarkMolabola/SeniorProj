using MDRNJfitness.Views;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Shows the how-to-play slides once per device, the first time someone reaches Home.
    /// Raise <see cref="Version"/> to show them again after a big change to the game.
    /// </summary>
    public static class TutorialGate
    {
        private const string Key = "tutorial_seen_version";
        private const int Version = 1;

        public static async Task ShowIfNewAsync()
        {
            if (Preferences.Default.Get(Key, 0) >= Version)
                return;

            await ShowAsync();
            Preferences.Default.Set(Key, Version);
        }

        public static Task ShowAsync() => ModalPages.PushAndWaitAsync(new TutorialPage());
    }
}
