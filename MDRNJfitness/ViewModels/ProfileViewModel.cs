using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Services.Interfaces;
using System.Collections.Generic;

namespace MDRNJfitness.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService _user;
        private readonly IGameService _game;
        private readonly IAuthService _auth;

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

        public ProfileViewModel(IUserService user, IGameService game, IAuthService auth)
        {
            _user = user;
            _game = game;
            _auth = auth;
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

        [RelayCommand]
        public async Task LogoutAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.LogoutAsync();
                await Shell.Current.GoToAsync("//LoginPage");
            });
    }
}
