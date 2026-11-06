using CommunityToolkit.Mvvm.Messaging;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;

namespace MDRNJfitness.Services
{
    /// <summary>
    /// The red numbers on the Guild tab and its Duels, Friends and Chat sub-tabs.
    /// Counts come from the server, so they stay right across devices and reinstalls.
    /// </summary>
    public sealed class NotificationBadges
    {
        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(750);

        private readonly IFriendService _friends;
        private readonly IChallengeService _challenges;
        private readonly IChatService _chat;
        private readonly ChatHubClient _chatHub;
        private int _generation;

        public int Duels { get; private set; }
        public int Friends { get; private set; }
        public int Chat { get; private set; }
        public int Guild => Duels + Friends + Chat;

        /// <summary>Raised on the main thread when any count changes.</summary>
        public event Action? Changed;

        public NotificationBadges(IFriendService friends, IChallengeService challenges, IChatService chat, ChatHubClient chatHub)
        {
            _friends = friends;
            _challenges = challenges;
            _chat = chat;
            _chatHub = chatHub;

            _chatHub.ChannelActivity += _ => Refresh();
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) =>
            {
                if (m.IsAboutChallenges || m.IsAboutFriends)
                    Refresh();
            });
        }

        /// <summary>Call once signed in: loads the counts and listens for new chat messages.</summary>
        public async Task StartAsync()
        {
            Refresh();
            try { await _chatHub.ConnectAsync(); }
            catch (Exception ex)
            {
                // Chat counts still update on the next refresh
                System.Diagnostics.Debug.WriteLine($"[badges] chat connect failed: {ex}");
            }
        }

        /// <summary>Reloads the counts shortly, so a burst of events costs one reload.</summary>
        public void Refresh() => _ = RefreshAsync(Interlocked.Increment(ref _generation));

        /// <summary>Signing out: clears the counts and stops listening as this user.</summary>
        public async Task ResetAsync()
        {
            Interlocked.Increment(ref _generation);
            Apply(0, 0, 0);
            await _chatHub.StopAsync();
        }

        private async Task RefreshAsync(int generation)
        {
            try
            {
                await Task.Delay(Delay);
                if (generation != _generation || string.IsNullOrEmpty(await TokenStore.GetAccessTokenAsync()))
                    return;

                var friends = _friends.GetPendingRequestsAsync();
                var duels = _challenges.GetMyChallengesAsync();
                var chats = _chat.GetChannelsAsync();
                await Task.WhenAll(friends, duels, chats);

                if (generation == _generation)
                    Apply(duels.Result.Count(c => c.CanRespond || !c.ResultSeen),
                          friends.Result.Count,
                          chats.Result.Sum(c => c.UnreadCount));
            }
            catch (Exception ex)
            {
                // Offline or server asleep: keep the last counts until the next refresh
                System.Diagnostics.Debug.WriteLine($"[badges] refresh failed: {ex}");
            }
        }

        private void Apply(int duels, int friends, int chat) =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Duels == duels && Friends == friends && Chat == chat) return;
                Duels = duels;
                Friends = friends;
                Chat = chat;
                Changed?.Invoke();
            });
    }
}
