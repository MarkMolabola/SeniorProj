using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>One chat: messages, the input bar, and (for groups) member management.</summary>
    public partial class ChatRoomViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IChatService _chat;
        private readonly IFriendService _friends;
        private readonly IAuthService _auth;
        private readonly ChatHubClient _hub;
        private Guid _myId;

        [ObservableProperty] private ChatChannelDto? _channel;
        [ObservableProperty] private string _draft = string.Empty;
        [ObservableProperty] private bool _hasOlder;
        [ObservableProperty] private bool _isSending;

        public ObservableCollection<ChatMessageItem> Messages { get; } = new();

        public int MaxMessageLength => ChatRules.MaxMessageLength;
        public bool IsGroup => Channel?.Kind == "Group";
        public bool IsDirect => Channel?.Kind == "Direct";
        public string Subtitle => Channel?.Kind switch
        {
            "Global" => "Everyone in MDRNJ Fit",
            "Group" => $"{Channel.MemberCount} members",
            _ => "Direct message"
        };

        /// <summary>Raised after messages load or arrive, so the page can scroll to the newest.</summary>
        public event Action? ScrollToEndRequested;

        public ChatRoomViewModel(IChatService chat, IFriendService friends, IAuthService auth, ChatHubClient hub)
        {
            _chat = chat;
            _friends = friends;
            _auth = auth;
            _hub = hub;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("channel", out var value) && value is ChatChannelDto channel)
                Channel = channel;
        }

        partial void OnChannelChanged(ChatChannelDto? value)
        {
            Title = value?.Name ?? "Chat";
            OnPropertyChanged(nameof(IsGroup));
            OnPropertyChanged(nameof(IsDirect));
            OnPropertyChanged(nameof(Subtitle));
        }

        public async Task AppearingAsync()
        {
            if (Channel is null) return;
            _myId = (await _auth.GetCurrentUserAsync())?.UserId ?? Guid.Empty;
            _hub.MessageReceived += OnMessageReceived;

            await RunSafeAsync(async () =>
            {
                var page = await _chat.GetMessagesAsync(Channel.ChannelId);
                Messages.Clear();
                foreach (var m in page) Messages.Add(ToItem(m));
                HasOlder = page.Count >= ChatRules.PageSize;
            });
            ScrollToEndRequested?.Invoke();

            try { await _hub.JoinChannelAsync(Channel.ChannelId); }
            catch (Exception ex)
            {
                // Live updates unavailable; reopening the chat still loads new messages
                System.Diagnostics.Debug.WriteLine($"[chat] join failed: {ex}");
            }
        }

        public async Task DisappearingAsync()
        {
            _hub.MessageReceived -= OnMessageReceived;
            if (Channel is not null)
                await _hub.LeaveChannelAsync(Channel.ChannelId);
        }

        private void OnMessageReceived(ChatMessageDto message)
        {
            if (message.ChannelId != Channel?.ChannelId) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Add(message)) return;
                ScrollToEndRequested?.Invoke();
                if (message.SenderId != _myId)
                    _ = MarkReadQuietlyAsync();
            });
        }

        [RelayCommand]
        public async Task SendAsync()
        {
            var text = Draft.Trim();
            if (Channel is null || text.Length == 0 || IsSending) return;

            IsSending = true;
            Draft = string.Empty;
            try
            {
                var sent = await _chat.SendMessageAsync(Channel.ChannelId, text);
                if (Add(sent))
                    ScrollToEndRequested?.Invoke();
            }
            catch (Exception ex)
            {
                Draft = text; // keep what they typed
                await Shell.Current.DisplayAlertAsync("Not sent", ex.Message, "OK");
            }
            finally
            {
                IsSending = false;
            }
        }

        [RelayCommand]
        public async Task LoadOlderAsync() =>
            await RunSafeAsync(async () =>
            {
                if (Channel is null || Messages.Count == 0) return;
                var page = await _chat.GetMessagesAsync(Channel.ChannelId, Messages[0].Message.SentAt);
                for (var i = page.Count - 1; i >= 0; i--)
                    Messages.Insert(0, ToItem(page[i]));
                HasOlder = page.Count >= ChatRules.PageSize;
            });

        /// <summary>Tap a name to see that player's profile (and add them as a friend).</summary>
        [RelayCommand]
        public async Task OpenSenderProfileAsync(ChatMessageItem item)
        {
            if (item.IsMine) return;
            await Shell.Current.GoToAsync($"FriendProfilePage?userId={item.Message.SenderId}");
        }

        [RelayCommand]
        public async Task OpenOtherProfileAsync()
        {
            if (Channel?.OtherUserId is Guid id)
                await Shell.Current.GoToAsync($"FriendProfilePage?userId={id}");
        }

        [RelayCommand]
        public async Task GroupMenuAsync()
        {
            if (Channel is null || !IsGroup) return;

            const string add = "Add a friend", members = "View members", leave = "Leave group";
            var choice = await Shell.Current.DisplayActionSheetAsync(Channel.Name, "Cancel", leave, add, members);
            if (choice == add) await AddFriendToGroupAsync();
            else if (choice == members) await ShowMembersAsync();
            else if (choice == leave) await LeaveGroupAsync();
        }

        private async Task AddFriendToGroupAsync() =>
            await RunSafeAsync(async () =>
            {
                var members = await _chat.GetMembersAsync(Channel!.ChannelId);
                var memberIds = members.Select(m => m.UserId).ToHashSet();
                var candidates = (await _friends.GetFriendsAsync()).Where(f => !memberIds.Contains(f.UserId)).ToList();
                if (candidates.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Add a friend", "All your friends are already in this group.", "OK");
                    return;
                }

                var pick = await Shell.Current.DisplayActionSheetAsync("Add to group", "Cancel", null,
                    candidates.Select(f => f.Username).OrderBy(n => n).ToArray());
                var friend = candidates.FirstOrDefault(f => f.Username == pick);
                if (friend is null) return;

                var updated = await _chat.AddMembersAsync(Channel.ChannelId, new List<Guid> { friend.UserId });
                Channel.MemberCount = updated.Count;
                OnPropertyChanged(nameof(Subtitle));
            });

        private async Task ShowMembersAsync() =>
            await RunSafeAsync(async () =>
            {
                var members = await _chat.GetMembersAsync(Channel!.ChannelId);
                var lines = members.Select(m => m.IsOwner ? $"★ {m.Username} (owner)" : m.Username);
                await Shell.Current.DisplayAlertAsync($"{members.Count} MEMBERS", string.Join("\n", lines), "OK");
            });

        private async Task LeaveGroupAsync() =>
            await RunSafeAsync(async () =>
            {
                var confirm = await Shell.Current.DisplayAlertAsync("Leave group",
                    $"Leave {Channel!.Name}? If you're the last member, the group is deleted.", "Leave", "Cancel");
                if (!confirm) return;

                await _chat.LeaveGroupAsync(Channel.ChannelId);
                await Shell.Current.GoToAsync("..");
            });

        private async Task MarkReadQuietlyAsync()
        {
            try { await _chat.MarkReadAsync(Channel!.ChannelId); }
            catch { /* unread badge just stays until next open */ }
        }

        /// <summary>Adds a message unless it's already shown (my sends arrive by REST and by the hub).</summary>
        private bool Add(ChatMessageDto message)
        {
            if (Messages.Any(m => m.Message.MessageId == message.MessageId)) return false;
            Messages.Add(ToItem(message));
            return true;
        }

        private ChatMessageItem ToItem(ChatMessageDto m) =>
            new(m, isMine: m.SenderId == _myId, showSender: !IsDirect);
    }
}
