using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class FriendProfileViewModel : BaseViewModel
    {
        private readonly IFriendService _friends;

        [ObservableProperty] private string _userId = string.Empty;
        [ObservableProperty] private FriendPublicProfileDto? _profile;

        public ObservableCollection<WorkoutDto> RecentWorkouts { get; } = new();

        public FriendProfileViewModel(IFriendService friends)
        {
            _friends = friends;
            Title = "Profile";
        }

        partial void OnUserIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
                LoadCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                if (!Guid.TryParse(UserId, out var id)) return;

                Profile = await _friends.GetFriendProfileAsync(id);
                Title = Profile.Username;

                RecentWorkouts.Clear();
                foreach (var w in Profile.RecentWorkouts)
                    RecentWorkouts.Add(w);
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
                await Shell.Current.GoToAsync("..");
            });
    }
}
