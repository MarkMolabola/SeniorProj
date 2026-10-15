using MDRNJ.Shared.DTOs.Auth;
using MDRNJfitness.Data;
using Newtonsoft.Json;
using System.Text;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Swaps the refresh token for a new access token. Singleton, so when
    /// several requests hit 401 at once only one refresh happens.
    /// </summary>
    public class TokenRefresher
    {
        // Not routed through AuthHeaderHandler: a 401 here must not trigger another refresh
        private readonly HttpClient _http = new(new TransientRetryHandler { InnerHandler = new HttpClientHandler() })
        {
            BaseAddress = new Uri(ApiConstants.BaseUrl)
        };
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly UserRepo _userRepo;

        public TokenRefresher(UserRepo userRepo) => _userRepo = userRepo;

        /// <summary>
        /// Gets a fresh access token. <paramref name="staleToken"/> is the one that just
        /// got a 401; if another request already replaced it, no new refresh is needed.
        /// </summary>
        public async Task<bool> TryRefreshAsync(string? staleToken)
        {
            await _gate.WaitAsync();
            try
            {
                var current = await TokenStore.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(current) && current != staleToken)
                    return true;

                var refreshToken = await TokenStore.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                var body = new StringContent(
                    JsonConvert.SerializeObject(new RefreshTokenDto { RefreshToken = refreshToken }),
                    Encoding.UTF8, "application/json");
                using var response = await _http.PostAsync("api/auth/refresh", body);
                if ((int)response.StatusCode >= 500)
                    return false; // server trouble, not a bad token: keep the session for next time

                if (!response.IsSuccessStatusCode)
                {
                    // Refresh token expired or revoked: this session is over
                    TokenStore.Clear();
                    await _userRepo.ClearAsync();
                    return false;
                }

                var auth = JsonConvert.DeserializeObject<AuthResponseDto>(await response.Content.ReadAsStringAsync());
                if (auth is null) return false;

                await TokenStore.SaveAsync(auth.AccessToken, auth.RefreshToken);
                if (auth.User is not null)
                    await _userRepo.CacheCurrentUserAsync(auth.User);
                return true;
            }
            catch (HttpRequestException)
            {
                return false; // offline: keep the tokens and try again later
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
