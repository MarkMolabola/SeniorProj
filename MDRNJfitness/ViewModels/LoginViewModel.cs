using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers;
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
        [ObservableProperty] private bool _acceptedTerms;

        public string TermsCheckboxText => $"I'm {LegalRules.MinimumAge} or older and agree to the";

        // Email verification step
        [ObservableProperty] private bool _isVerifyMode;
        [ObservableProperty] private string _verificationCode = string.Empty;
        [ObservableProperty] private string _verifyMessage = string.Empty;

        // Forgot password: ask for a code, then enter it with the new password
        [ObservableProperty] private bool _isResetMode;
        [ObservableProperty] private bool _isResetCodeSent;
        [ObservableProperty] private string _resetCode = string.Empty;
        [ObservableProperty] private string _newPassword = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;

        /// <summary>Sign in / register is showing (not verify or reset).</summary>
        public bool IsMainMode => !IsVerifyMode && !IsResetMode;
        partial void OnIsVerifyModeChanged(bool value) => OnPropertyChanged(nameof(IsMainMode));
        partial void OnIsResetModeChanged(bool value) => OnPropertyChanged(nameof(IsMainMode));

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
                    await GoHomeAsync();
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
                    await GoHomeAsync();
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
                if (!AcceptedTerms)
                {
                    await Shell.Current.DisplayAlertAsync("Terms of Service",
                        $"To create an account, confirm you're {LegalRules.MinimumAge} or older and agree to the Terms of Service and Privacy Policy.", "OK");
                    return;
                }

                var result = await _auth.RegisterAsync(new RegisterDto
                {
                    Email = Email,
                    Password = Password,
                    Username = Username,
                    DisplayName = DisplayName,
                    AcceptedTerms = true
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
                await GoHomeAsync();
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
                    await GoHomeAsync();
            });

        [RelayCommand]
        public Task OpenTermsAsync() => TermsGate.OpenTermsAsync();

        [RelayCommand]
        public Task OpenPrivacyAsync() => TermsGate.OpenPrivacyAsync();

        /// <summary>Into the app, once the player has accepted the current terms.</summary>
        private async Task<bool> GoHomeAsync()
        {
            if (!await TermsGate.EnsureAcceptedAsync(_auth))
            {
                BackToSignIn();
                return false;
            }
            await Shell.Current.GoToAsync("//DashboardPage");
            return true;
        }

        [RelayCommand]
        public void ForgotPassword()
        {
            IsResetMode = true;
            IsResetCodeSent = false;
            Title = "Reset Password";
        }

        [RelayCommand]
        public async Task SendResetCodeAsync() =>
            await RunSafeAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    await Shell.Current.DisplayAlertAsync("Email", "Enter the email you signed up with.", "OK");
                    return;
                }
                Email = Email.Trim();
                await _auth.ForgotPasswordAsync(Email);
                ResetCode = NewPassword = ConfirmPassword = string.Empty;
                IsResetCodeSent = true;
            });

        [RelayCommand]
        public async Task ResetPasswordAsync() =>
            await RunSafeAsync(async () =>
            {
                string? problem = ResetCode.Trim().Length != 6 ? "Enter the 6-digit code from the email."
                    : NewPassword.Length < 8 ? "Your new password needs at least 8 characters."
                    : NewPassword != ConfirmPassword ? "The two passwords don't match."
                    : null;
                if (problem is not null)
                {
                    await Shell.Current.DisplayAlertAsync("Reset password", problem, "OK");
                    return;
                }

                await _auth.ResetPasswordAsync(new ResetPasswordDto
                {
                    Email = Email,
                    Code = ResetCode.Trim(),
                    NewPassword = NewPassword
                });
                BackToSignIn();
                Password = string.Empty;
                if (await GoHomeAsync())
                    await Shell.Current.DisplayAlertAsync("Password changed",
                        "You're signed in with your new password. Any other devices were signed out.", "OK");
            });

        [RelayCommand]
        public void BackToSignIn()
        {
            IsVerifyMode = false;
            IsRegisterMode = false;
            IsResetMode = false;
            IsResetCodeSent = false;
            VerificationCode = ResetCode = NewPassword = ConfirmPassword = string.Empty;
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
