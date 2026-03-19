using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Services.Interfaces;

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
        public async Task LogoutAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.LogoutAsync();
                await Shell.Current.GoToAsync("//LoginPage");
            });
    }
}
