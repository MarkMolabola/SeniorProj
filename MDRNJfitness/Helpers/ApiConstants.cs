namespace MDRNJfitness.Helpers
{
    public static class ApiConstants
    {
#if DEBUG && ANDROID
        public const string BaseUrl = "http://10.0.2.2:5215/";
#elif DEBUG
        public const string BaseUrl = "http://localhost:5215/";
#else
        public const string BaseUrl = "https://mdrn-j-fitness-efgabvesehgqc0fy.westus-01.azurewebsites.net/";
#endif

        // Custom URL scheme the API redirects to after Google sign-in.
        // Must match the Android callback activity and iOS Info.plist URL type.
        public const string AuthCallbackUrl = "mdrnjfitness://auth";
    }
}
