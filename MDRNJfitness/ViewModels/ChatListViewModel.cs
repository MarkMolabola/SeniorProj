using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Guild ▸ Chat: the global room, your groups/guilds and your direct chats.</summary>
    public partial class ChatListViewModel : BaseViewModel
    {
        private readonly IChatService _chat;
        private readonly IFriendService _friends;
        private readonly ChatHubClient _hub;
        private bool _visible;

        public ObservableCollection<ChatChannelCard> Channels { get; } = new();

        public ChatListViewModel(IChatService chat, IFriendService friends, ChatHubClient hub)
        {
            _chat = chat;
            _friends = friends;
            _hub = hub;
            _hub.ChannelActivity += OnChannelActivity;
            Title = "Chat";
        }

        public async Task AppearingAsync()
        {
            _visible = true;
            await LoadAsync();
            try { await _hub.ConnectAsync(); }
            catch (Exception ex)
            {
                // Live updates unavailable; the list still reloads each time it opens
                System.Diagnostics.Debug.WriteLine($"[chat] connect failed: {ex}");
            }
        }

        public void Disappearing() => _visible = false;

        private void OnChannelActivity(Guid channelId)
        {
            if (_visible)
                MainThread.BeginInvokeOnMainThread(() => LoadCommand.Execute(null));
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var channels = await _chat.GetChannelsAsync();
                Channels.Clear();
                foreach (var c in channels)
                    Channels.Add(new ChatChannelCard(c));
            });

        [RelayCommand]
        public Task OpenAsync(ChatChannelCard card) => OpenChannelAsync(card.Channel);

        [RelayCommand]
        public async Task NewGroupAsync() =>
            await RunSafeAsync(async () =>
            {
                var name = await Shell.Current.DisplayPromptAsync(
                    "NEW GROUP", "Name your group or guild. You can add friends once it's made.",
                    "CREATE", "Cancel", "e.g. Iron Guild", ChatRules.MaxGroupNameLength);
                if (string.IsNullOrWhiteSpace(name)) return;

                var channel = await _chat.CreateGroupAsync(name.Trim(), new List<Guid>());
                await OpenChannelAsync(channel);
            });

        [RelayCommand]
        public async Task NewDirectAsync() =>
            await RunSafeAsync(async () =>
            {
                var friends = await _friends.GetFriendsAsync();
                if (friends.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("No friends yet",
                        "Add friends from the Friends tab or the Ranks, then message them here.", "OK");
                    return;
                }

                var names = friends.Select(f => f.Username).OrderBy(n => n).ToArray();
                var pick = await Shell.Current.DisplayActionSheetAsync("Message a friend", "Cancel", null, names);
                var friend = friends.FirstOrDefault(f => f.Username == pick);
                if (friend is null) return;

                await OpenChannelAsync(await _chat.OpenDirectAsync(friend.UserId));
            });

        private static Task OpenChannelAsync(ChatChannelDto channel) =>
            Shell.Current.GoToAsync("ChatRoomPage", new ShellNavigationQueryParameters { ["channel"] = channel });
    }
}
