namespace MDRNJfitness.Helpers
{
    public static class ApiConstants
    {
#if ANDROID
        public const string BaseUrl = "http://10.0.2.2:5215/";
#else
        public const string BaseUrl = "http://localhost:5215/";
#endif
    }
}
