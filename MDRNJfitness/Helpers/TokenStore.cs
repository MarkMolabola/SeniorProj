namespace MDRNJfitness.Helpers
{
    /// <summary>The signed-in session's tokens, kept in the platform's secure storage.</summary>
    public static class TokenStore
    {
        private const string AccessKey = "access_token";
        private const string RefreshKey = "refresh_token";

        public static Task<string?> GetAccessTokenAsync() => SecureStorage.GetAsync(AccessKey);
        public static Task<string?> GetRefreshTokenAsync() => SecureStorage.GetAsync(RefreshKey);

        public static async Task SaveAsync(string accessToken, string? refreshToken)
        {
            await SecureStorage.SetAsync(AccessKey, accessToken);
            if (!string.IsNullOrEmpty(refreshToken))
                await SecureStorage.SetAsync(RefreshKey, refreshToken);
        }

        public static void Clear()
        {
            SecureStorage.Remove(AccessKey);
            SecureStorage.Remove(RefreshKey);
        }
    }
}
