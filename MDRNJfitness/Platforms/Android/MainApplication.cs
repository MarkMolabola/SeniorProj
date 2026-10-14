using Android.App;
using Android.Runtime;

namespace MDRNJfitness
{
#if DEBUG
    // Debug builds talk to the local API over plain http://10.0.2.2. Android blocks cleartext
    // by default, which stopped SignalR (live chat/leaderboard) from connecting. Release uses https.
    [Application(UsesCleartextTraffic = true)]
#else
    [Application]
#endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
