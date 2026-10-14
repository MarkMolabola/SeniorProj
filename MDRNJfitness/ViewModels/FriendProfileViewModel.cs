using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>
    /// Any player's profile (from Friends, the Ranks or chat). The bottom buttons follow
    /// the relationship: add, request sent, accept, or message/remove for friends.
    /// </summary>
    [QueryProperty(nameof(UserId), "userId")]
    public partial class FriendProfileViewModel : BaseViewModel
    {
        private readonly IFriendService _friends;
        private readonly IChatService _chat;

        [ObservableProperty] private string _userId = string.Empty;
        [ObservableProperty] private FriendPublicProfileDto? _profile;

        public ObservableCollection<WorkoutDto> RecentWorkouts { get; } = new();

        private string Relationship => Profile?.Relationship ?? FriendRelationship.None;
        public bool IsFriend => Relationship == FriendRelationship.Friends;
        public bool CanAddFriend => Relationship == FriendRelationship.None;
        public bool IsRequestSent => Relationship == FriendRelationship.RequestSent;
        public bool IsRequestReceived => Relationship == FriendRelationship.RequestReceived;
        public bool ShowsWorkouts => IsFriend || Relationship == FriendRelationship.Self;
        public bool HidesWorkouts => Profile is not null && !ShowsWorkouts;

        public FriendProfileViewModel(IFriendService friends, IChatService chat)
        {
            _friends = friends;
            _chat = chat;
            Title = "Profile";
        }

        partial void OnUserIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
                LoadCommand.Execute(null);
        }

        partial void OnProfileChanged(FriendPublicProfileDto? value)
        {
            OnPropertyChanged(nameof(IsFriend));
            OnPropertyChanged(nameof(CanAddFriend));
            OnPropertyChanged(nameof(IsRequestSent));
            OnPropertyChanged(nameof(IsRequestReceived));
            OnPropertyChanged(nameof(ShowsWorkouts));
            OnPropertyChanged(nameof(HidesWorkouts));
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(ReloadAsync);

        private async Task ReloadAsync()
        {
            if (!Guid.TryParse(UserId, out var id)) return;

            Profile = await _friends.GetFriendProfileAsync(id);
            Title = Profile.Username;

            RecentWorkouts.Clear();
            foreach (var w in Profile.RecentWorkouts)
                RecentWorkouts.Add(w);
        }

        [RelayCommand]
        public async Task AddFriendAsync() =>
            await RunSafeAsync(async () =>
            {
                if (Profile is null) return;
                await _friends.SendRequestAsync(Profile.Username);
                await ReloadAsync();
            });

        [RelayCommand]
        public async Task AcceptRequestAsync() =>
            await RunSafeAsync(async () =>
            {
                if (Profile?.PendingFriendshipId is not Guid friendshipId) return;
                await _friends.AcceptRequestAsync(friendshipId);
                await ReloadAsync();
            });

        [RelayCommand]
        public async Task MessageAsync() =>
            await RunSafeAsync(async () =>
            {
                if (Profile is null) return;
                var channel = await _chat.OpenDirectAsync(Profile.UserId);
                await Shell.Current.GoToAsync("ChatRoomPage", new ShellNavigationQueryParameters { ["channel"] = channel });
            });

        [RelayCommand]
        public async Task RemoveFriendAsync() =>
            await RunSafeAsync(async () =>
            {
                if (!Guid.TryParse(UserId, out var id)) return;

                var confirm = await Shell.Current.DisplayAlertAsync(
                    "Remove Friend",
                    $"Remove {Profile?.Username} from your friends?",
                    "Remove", "Cancel");
                if (!confirm) return;

                await _friends.RemoveFriendAsync(id);
                await ReloadAsync();
            });
    }
}
