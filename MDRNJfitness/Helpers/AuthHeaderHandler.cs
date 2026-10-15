using System.Net;
using System.Net.Http.Headers;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Adds the access token to API requests. When the API answers 401 (token
    /// expired), refreshes the session once and retries; if that fails, sends
    /// the user back to the login screen.
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly TokenRefresher _refresher;

        public AuthHeaderHandler(TokenRefresher refresher) => _refresher = refresher;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Buffer the body so the request can be sent a second time
            var body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);

            var token = await TokenStore.GetAccessTokenAsync();
            Authorize(request, token);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized || IsAuthEndpoint(request) || string.IsNullOrEmpty(token))
                return response;

            if (!await _refresher.TryRefreshAsync(token))
            {
                SendToLogin();
                return response;
            }

            response.Dispose();
            using var retry = TransientRetryHandler.Clone(request, body);
            Authorize(retry, await TokenStore.GetAccessTokenAsync());
            return await base.SendAsync(retry, cancellationToken);
        }

        private static void Authorize(HttpRequestMessage request, string? token)
        {
            request.Headers.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>Login / register / refresh answer 401 for bad credentials; refreshing won't help.</summary>
        private static bool IsAuthEndpoint(HttpRequestMessage request) =>
            request.RequestUri?.AbsolutePath.Contains("/api/auth/", StringComparison.OrdinalIgnoreCase) == true;

        private static void SendToLogin() =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Shell.Current is not null)
                    await Shell.Current.GoToAsync("//LoginPage");
            });
    }
}
