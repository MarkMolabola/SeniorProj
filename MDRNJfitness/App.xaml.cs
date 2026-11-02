using MDRNJfitness.Helpers;
using MDRNJfitness.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace MDRNJfitness
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Friends see you as online only while the app is in front: drop the live
            // connection in the background and pick it back up on return (if signed in).
            // A running workout still shows as working out; that comes from the server.
            window.Stopped += async (_, _) =>
            {
                if (Notifications is { } hub)
                    await hub.StopAsync();
            };
            window.Resumed += async (_, _) =>
            {
                if (Notifications is { } hub && !string.IsNullOrEmpty(await TokenStore.GetAccessTokenAsync()))
                    await hub.ConnectAsync();
            };
            return window;
        }

        private static NotificationHubClient? Notifications =>
            IPlatformApplication.Current?.Services.GetService<NotificationHubClient>();
    }
}
