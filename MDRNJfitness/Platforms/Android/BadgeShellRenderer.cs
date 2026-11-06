using Google.Android.Material.Badge;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Tabs;
using MDRNJfitness.Services;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AColor = Android.Graphics.Color;

namespace MDRNJfitness.Platforms.Android;

/// <summary>Draws <see cref="NotificationBadges"/> as red numbers on the native tabs.</summary>
public class BadgeShellRenderer : ShellRenderer
{
    private const string GuildRoute = "guild";
    private const int MaxDigits = 3; // 100 shows as 99+
    private static readonly AColor BadgeRed = AColor.ParseColor("#E5383B");

    private static NotificationBadges? Badges =>
        IPlatformApplication.Current?.Services.GetService<NotificationBadges>();

    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem) =>
        new BottomBadgeTracker(this, shellItem);

    protected override IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection) =>
        new TopBadgeTracker(this, shellSection);

    private static void Style(BadgeDrawable badge, int count)
    {
        badge.BackgroundColor = BadgeRed;
        badge.BadgeTextColor = AColor.White;
        badge.MaxCharacterCount = MaxDigits;
        badge.Number = count;
        badge.SetVisible(true);
    }

    private sealed class BottomBadgeTracker : ShellBottomNavViewAppearanceTracker
    {
        private readonly ShellItem _item;
        private BottomNavigationView? _view;

        public BottomBadgeTracker(IShellContext context, ShellItem item) : base(context, item) => _item = item;

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);
            Attach(bottomView);
        }

        public override void ResetAppearance(BottomNavigationView bottomView)
        {
            base.ResetAppearance(bottomView);
            Attach(bottomView);
        }

        private void Attach(BottomNavigationView view)
        {
            if (_view is null && Badges is { } badges)
                badges.Changed += Update;
            _view = view;
            view.Post(Update);
        }

        private void Update()
        {
            var index = _item.Items.ToList().FindIndex(s => s.Route == GuildRoute);
            if (_view is null || index < 0 || Badges is not { } badges) return;

            if (badges.Guild > 0)
                Style(_view.GetOrCreateBadge(index), badges.Guild);
            else
                _view.RemoveBadge(index);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Badges is { } badges)
                badges.Changed -= Update;
            base.Dispose(disposing);
        }
    }

    private sealed class TopBadgeTracker : ShellTabLayoutAppearanceTracker
    {
        private readonly ShellSection _section;
        private TabLayout? _view;

        public TopBadgeTracker(IShellContext context, ShellSection section) : base(context) => _section = section;

        public override void SetAppearance(TabLayout tabLayout, ShellAppearance appearance)
        {
            base.SetAppearance(tabLayout, appearance);
            Attach(tabLayout);
        }

        public override void ResetAppearance(TabLayout tabLayout)
        {
            base.ResetAppearance(tabLayout);
            Attach(tabLayout);
        }

        private void Attach(TabLayout view)
        {
            if (_section.Route != GuildRoute) return;
            if (_view is null && Badges is { } badges)
                badges.Changed += Update;
            _view = view;
            view.Post(Update);
        }

        private void Update()
        {
            if (_view is null || Badges is not { } badges) return;
            Show("ChallengePage", badges.Duels);
            Show("FriendsPage", badges.Friends);
            Show("ChatPage", badges.Chat);
        }

        private void Show(string route, int count)
        {
            var index = _section.Items.ToList().FindIndex(c => c.Route == route);
            if (index < 0 || _view?.GetTabAt(index) is not { } tab) return;

            if (count > 0)
                Style(tab.OrCreateBadge, count);
            else
                tab.RemoveBadge();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Badges is { } badges)
                badges.Changed -= Update;
            base.Dispose(disposing);
        }
    }
}
