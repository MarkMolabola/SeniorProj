using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    public partial class FriendsViewModel : BaseViewModel
    {
        private readonly IFriendService _friends;

        [ObservableProperty] private string _searchUsername = string.Empty;
        public ObservableCollection<FriendDto> Friends { get; } = new();
        public ObservableCollection<FriendDto> PendingRequests { get; } = new();

        public FriendsViewModel(IFriendService friends)
        {
            _friends = friends;
            Title = "Friends";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                var friends = await _friends.GetFriendsAsync();
                Friends.Clear();
                foreach (var f in friends) Friends.Add(f);

                var pending = await _friends.GetPendingRequestsAsync();
                PendingRequests.Clear();
                foreach (var p in pending) PendingRequests.Add(p);
            });

        [RelayCommand]
        public async Task SendRequestAsync() =>
            await RunSafeAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(SearchUsername)) return;
                var result = await _friends.SendRequestAsync(SearchUsername.Trim());
                SearchUsername = string.Empty;
                await Shell.Current.DisplayAlertAsync("Sent", $"Friend request sent to {result.Username}.", "OK");
            });

        [RelayCommand]
        public async Task AcceptAsync(FriendDto friend) =>
            await RunSafeAsync(async () =>
            {
                var accepted = await _friends.AcceptRequestAsync(friend.FriendshipId);
                PendingRequests.Remove(friend);
                Friends.Add(accepted);
            });

        [RelayCommand]
        public async Task ViewProfileAsync(FriendDto friend) =>
            await Shell.Current.GoToAsync($"FriendProfilePage?userId={friend.UserId}");

        [RelayCommand]
        public async Task DeclineAsync(FriendDto friend) =>
            await RunSafeAsync(async () =>
            {
                await _friends.DeclineRequestAsync(friend.FriendshipId);
                PendingRequests.Remove(friend);
            });
    }
}
