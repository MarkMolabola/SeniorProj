using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Helpers;
using MDRNJfitness.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MDRNJfitness.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService _user;
        private readonly IGameService _game;
        private readonly IAuthService _auth;
        private readonly IFriendService _friends;

        [ObservableProperty] private UserDto? _currentUser;
        [ObservableProperty] private UserProfileDto? _profile;
        [ObservableProperty] private GameStatsDto? _stats;

        // Edit mode
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotEditing))]
        private bool _isEditing;
        public bool IsNotEditing => !IsEditing;

        [ObservableProperty] private string _editFitnessGoal = string.Empty;
        [ObservableProperty] private string _editWeight = string.Empty;
        [ObservableProperty] private string _editHeight = string.Empty;

        public List<string> FitnessGoals { get; } = new()
        {
            "Lose Weight", "Build Muscle", "Improve Endurance",
            "Stay Active", "Increase Flexibility", "General Fitness"
        };

        public ProfileViewModel(IUserService user, IGameService game, IAuthService auth, IFriendService friends)
        {
            _user = user;
            _game = game;
            _auth = auth;
            _friends = friends;
            Title = "Profile";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () =>
            {
                CurrentUser = await _user.GetCurrentUserAsync();
                Profile = await _user.GetProfileAsync();
                Stats = await _game.GetStatsAsync();
            });

        [RelayCommand]
        public void StartEdit()
        {
            EditFitnessGoal = Profile?.FitnessGoal ?? string.Empty;
            EditWeight = Profile?.WeightKg?.ToString("F1") ?? string.Empty;
            EditHeight = Profile?.HeightCm?.ToString("F1") ?? string.Empty;
            IsEditing = true;
        }

        [RelayCommand]
        public void CancelEdit() => IsEditing = false;

        [RelayCommand]
        public async Task SaveProfileAsync() =>
            await RunSafeAsync(async () =>
            {
                float.TryParse(EditWeight, out var weight);
                float.TryParse(EditHeight, out var height);

                Profile = await _user.UpdateProfileAsync(new UpdateUserProfileDto
                {
                    DisplayName = Profile?.DisplayName ?? string.Empty,
                    Bio = Profile?.Bio,
                    FitnessGoal = EditFitnessGoal,
                    WeightKg = weight > 0 ? weight : null,
                    HeightCm = height > 0 ? height : null,
                });

                IsEditing = false;
            });

        /// <summary>Lists blocked players; pick one to unblock.</summary>
        [RelayCommand]
        public async Task ManageBlockedAsync() =>
            await RunSafeAsync(async () =>
            {
                var blocked = await _friends.GetBlockedAsync();
                if (blocked.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Blocked players",
                        "You haven't blocked anyone. Tap a chat message or open a player's profile to block them.", "OK");
                    return;
                }

                var pick = await Shell.Current.DisplayActionSheetAsync("Tap a player to unblock", "Close", null,
                    blocked.Select(b => b.Username).ToArray());
                var player = blocked.FirstOrDefault(b => b.Username == pick);
                if (player is null) return;

                if (await Shell.Current.DisplayAlertAsync("Unblock", $"Unblock {player.Username}?", "Unblock", "Cancel"))
                    await _friends.UnblockAsync(player.UserId);
            });

        /// <summary>Permanently deletes the account after two confirmations.</summary>
        [RelayCommand]
        public async Task DeleteAccountAsync() =>
            await RunSafeAsync(async () =>
            {
                if (!await Shell.Current.DisplayAlertAsync("Delete account",
                        "This permanently deletes your hero, stats, workouts, gold, gear, friends and messages. It can't be undone.",
                        "Continue", "Cancel"))
                    return;

                var typed = await Shell.Current.DisplayPromptAsync("Are you sure?",
                    "Type DELETE to permanently delete your account.", "DELETE", "Cancel", "DELETE", 10);
                if (!string.Equals(typed?.Trim(), "DELETE", StringComparison.Ordinal))
                    return;

                await _auth.DeleteAccountAsync();
                await Shell.Current.DisplayAlertAsync("Account deleted", "Your account has been deleted. Thanks for training with us.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            });

        [RelayCommand]
        public Task OpenTermsAsync() => TermsGate.OpenTermsAsync();

        [RelayCommand]
        public Task OpenPrivacyAsync() => TermsGate.OpenPrivacyAsync();

        [RelayCommand]
        public async Task LogoutAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.LogoutAsync();
                await Shell.Current.GoToAsync("//LoginPage");
            });
    }
}
