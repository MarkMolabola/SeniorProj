using MDRNJfitness.Services;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace MDRNJfitness.Platforms.iOS;

/// <summary>
/// Shows <see cref="NotificationBadges"/>: a native red number on the Guild tab, and the count
/// in the sub-tab titles, because iOS Shell's top tabs have no native badge.
/// </summary>
public class BadgeShellRenderer : ShellRenderer
{
    private const string GuildRoute = "guild";

    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker() => new BadgeTracker();

    private sealed class BadgeTracker : ShellTabBarAppearanceTracker
    {
        private static readonly Dictionary<string, string> Titles = new();
        private UITabBarController? _controller;

        private static NotificationBadges? Badges =>
            IPlatformApplication.Current?.Services.GetService<NotificationBadges>();

        public override void UpdateLayout(UITabBarController controller)
        {
            base.UpdateLayout(controller);
            if (_controller is null && Badges is { } badges)
                badges.Changed += Update;
            _controller = controller;
            Update();
        }

        private void Update()
        {
            if (Badges is not { } badges || Shell.Current?.CurrentItem is not { } item) return;

            var index = item.Items.ToList().FindIndex(s => s.Route == GuildRoute);
            if (index < 0) return;

            var tabs = _controller?.TabBar.Items;
            if (tabs is not null && index < tabs.Length)
                tabs[index].BadgeValue = badges.Guild > 0 ? Short(badges.Guild) : null;

            var guild = item.Items[index];
            SetTitle(guild, "ChallengePage", badges.Duels);
            SetTitle(guild, "FriendsPage", badges.Friends);
            SetTitle(guild, "ChatPage", badges.Chat);
        }

        private static void SetTitle(ShellSection guild, string route, int count)
        {
            if (guild.Items.FirstOrDefault(c => c.Route == route) is not { } page) return;
            if (!Titles.TryGetValue(route, out var title))
                Titles[route] = title = page.Title;

            var shown = count > 0 ? $"{title} ({Short(count)})" : title;
            if (page.Title != shown)
                page.Title = shown;
        }

        private static string Short(int count) => count > 99 ? "99+" : count.ToString();

        protected override void Dispose(bool disposing)
        {
            if (disposing && Badges is { } badges)
                badges.Changed -= Update;
            base.Dispose(disposing);
        }
    }
}
