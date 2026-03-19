using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Auth;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _auth;

        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _username = string.Empty;
        [ObservableProperty] private string _displayName = string.Empty;
        [ObservableProperty] private bool _isRegisterMode;

        public LoginViewModel(IAuthService auth)
        {
            _auth = auth;
            Title = "Sign In";
        }

        [RelayCommand]
        public async Task LoginAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.LoginAsync(new LoginDto
                {
                    Email = Email,
                    Password = Password
                });
                await Shell.Current.GoToAsync("//DashboardPage");
            });

        [RelayCommand]
        public async Task RegisterAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.RegisterAsync(new RegisterDto
                {
                    Email = Email,
                    Password = Password,
                    Username = Username,
                    DisplayName = DisplayName
                });
                await Shell.Current.GoToAsync("//DashboardPage");
            });

        [RelayCommand]
        public void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            Title = IsRegisterMode ? "Register" : "Sign In";
        }
    }
}
