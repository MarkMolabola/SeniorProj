using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Auth;
using MDRNJfitness.Services.Interfaces;
using System.Net;

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

        // Email verification step
        [ObservableProperty] private bool _isVerifyMode;
        [ObservableProperty] private string _verificationCode = string.Empty;
        [ObservableProperty] private string _verifyMessage = string.Empty;

        // WebAuthenticator isn't supported on Windows desktop.
        public bool IsGoogleSignInSupported => DeviceInfo.Platform != DevicePlatform.WinUI;

        public LoginViewModel(IAuthService auth)
        {
            _auth = auth;
            Title = "Sign In";
        }

        private bool _triedResume;

        /// <summary>Skips the login screen when a saved session is still valid (once per launch).</summary>
        public async Task TryResumeSessionAsync()
        {
            if (_triedResume) return;
            _triedResume = true;

            await RunSafeAsync(async () =>
            {
                if (await _auth.TryResumeSessionAsync())
                    await Shell.Current.GoToAsync("//DashboardPage");
            });
        }

        [RelayCommand]
        public async Task LoginAsync() =>
            await RunSafeAsync(async () =>
            {
                try
                {
                    await _auth.LoginAsync(new LoginDto
                    {
                        Email = Email,
                        Password = Password
                    });
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Correct password but email not verified — the API just sent a code.
                    ShowVerifyStep(ex.Message);
                }
            });

        [RelayCommand]
        public async Task RegisterAsync() =>
            await RunSafeAsync(async () =>
            {
                var result = await _auth.RegisterAsync(new RegisterDto
                {
                    Email = Email,
                    Password = Password,
                    Username = Username,
                    DisplayName = DisplayName
                });
                ShowVerifyStep(result.Message);
            });

        [RelayCommand]
        public async Task VerifyEmailAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.VerifyEmailAsync(new VerifyEmailDto
                {
                    Email = Email,
                    Code = VerificationCode.Trim()
                });
                IsVerifyMode = false;
                await Shell.Current.GoToAsync("//DashboardPage");
            });

        [RelayCommand]
        public async Task ResendCodeAsync() =>
            await RunSafeAsync(async () =>
            {
                await _auth.ResendVerificationAsync(Email);
                VerificationCode = string.Empty;
                VerifyMessage = $"We sent a new code to {Email}.";
            });

        [RelayCommand]
        public async Task SignInWithGoogleAsync() =>
            await RunSafeAsync(async () =>
            {
                if (await _auth.SignInWithGoogleAsync())
                    await Shell.Current.GoToAsync("//DashboardPage");
            });

        [RelayCommand]
        public void BackToSignIn()
        {
            IsVerifyMode = false;
            IsRegisterMode = false;
            VerificationCode = string.Empty;
            Title = "Sign In";
        }

        [RelayCommand]
        public void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            Title = IsRegisterMode ? "Register" : "Sign In";
        }

        private void ShowVerifyStep(string message)
        {
            VerifyMessage = message;
            VerificationCode = string.Empty;
            IsVerifyMode = true;
            Title = "Verify Email";
        }
    }
}
