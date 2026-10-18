namespace MDRNJfitness.Helpers
{
    public static class ApiConstants
    {
        /// <summary>
        /// Release builds always use Azure. Debug builds use the local API unless built with
        /// UseAzureApi=true: `dotnet build -p:UseAzureApi=true`, an environment variable
        /// UseAzureApi=true, or &lt;UseAzureApi&gt;true&lt;/UseAzureApi&gt; in MDRNJfitness.csproj.user (not committed).
        /// </summary>
        public const string AzureUrl = "https://mdrn-j-fitness-efgabvesehgqc0fy.westus-01.azurewebsites.net/";

#if !DEBUG || AZURE_API
        public const string BaseUrl = AzureUrl;
#elif ANDROID
        public const string BaseUrl = "http://10.0.2.2:5215/";
#else
        public const string BaseUrl = "http://localhost:5215/";
#endif

        // Custom URL scheme the API redirects to after Google sign-in.
        // Must match the Android callback activity and iOS Info.plist URL type.
        public const string AuthCallbackUrl = "mdrnjfitness://auth";
    }
}
