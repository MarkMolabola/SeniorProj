using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Data;
using MDRNJfitness.Helpers;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;
using MDRNJfitness.SignalR;

namespace MDRNJfitness.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly UserRepo _userRepo;
        private readonly TokenRefresher _refresher;
        private readonly NotificationHubClient _notifications;

        public AuthService(HttpClient http, UserRepo userRepo, TokenRefresher refresher, NotificationHubClient notifications)
        {
            _http = http;
            _userRepo = userRepo;
            _refresher = refresher;
            _notifications = notifications;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsync<AuthResponseDto>("api/auth/login", dto);
            await SaveSessionAsync(response);
            return response!;
        }

        // Registration doesn't log in — the user must enter the emailed code first.
        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto) =>
            (await _http.PostAsync<RegisterResponseDto>("api/auth/register", dto))!;

        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto)
        {
            var response = await _http.PostAsync<AuthResponseDto>("api/auth/verify-email", dto);
            await SaveSessionAsync(response);
            return response!;
        }

        public Task ResendVerificationAsync(string email) =>
            _http.PostAsync("api/auth/resend-verification", new ResendVerificationDto { Email = email });

        public async Task<bool> SignInWithGoogleAsync()
        {
            WebAuthenticatorResult result;
            try
            {
                // Opens the API's Google endpoint in a browser tab. When done, the API redirects to
                // mdrnjfitness://auth#access_token=... and the OS hands that URL back to the app.
                result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(new Uri(ApiConstants.BaseUrl), "api/auth/google"),
                    new Uri(ApiConstants.AuthCallbackUrl));
            }
            catch (TaskCanceledException)
            {
                return false; // user closed the browser
            }

            if (result.Properties.TryGetValue("error", out var error))
            {
                if (error == "google_sign_in_cancelled")
                    return false;
                throw new InvalidOperationException(error == "google_not_configured"
                    ? "Google sign-in isn't set up on the server yet."
                    : error);
            }

            var token = result.AccessToken
                ?? throw new InvalidOperationException("Google sign-in didn't return a token.");

            await TokenStore.SaveAsync(token, result.RefreshToken);
            var user = await _http.GetAsync<UserDto>("api/users/me");
            if (user is not null)
                await _userRepo.CacheCurrentUserAsync(user);

            return true;
        }

        public async Task LogoutAsync()
        {
            // Revoke the refresh token so it can't be used again (best effort: still sign out offline)
            var refreshToken = await TokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try { await _http.PostAsync("api/auth/logout", new RefreshTokenDto { RefreshToken = refreshToken }); }
                catch (HttpRequestException) { }
            }

            await _notifications.StopAsync();
            TokenStore.Clear();
            await _userRepo.ClearAsync();
        }

        public async Task DeleteAccountAsync()
        {
            await _http.DeleteAsync("api/users/me");
            await _notifications.StopAsync();
            TokenStore.Clear();
            await _userRepo.ClearAsync();
        }

        public async Task<bool> TryResumeSessionAsync()
        {
            if (string.IsNullOrEmpty(await TokenStore.GetRefreshTokenAsync()))
                return false;
            // Always refresh on launch: confirms the session is still valid and starts a fresh hour
            return await _refresher.TryRefreshAsync(await TokenStore.GetAccessTokenAsync() ?? string.Empty)
                && await _userRepo.GetCachedCurrentUserAsync() is not null;
        }

        public Task<UserDto?> GetCurrentUserAsync() =>
            _userRepo.GetCachedCurrentUserAsync();

        private async Task SaveSessionAsync(AuthResponseDto? response)
        {
            if (response is null) return;
            await TokenStore.SaveAsync(response.AccessToken, response.RefreshToken);
            if (response.User is not null)
                await _userRepo.CacheCurrentUserAsync(response.User);
        }
    }
}
