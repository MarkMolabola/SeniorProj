using Android.App;
using Android.Content;
using Android.Content.PM;

namespace MDRNJfitness
{
    // Receives the mdrnjfitness://auth redirect after Google sign-in and hands it to WebAuthenticator.
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "mdrnjfitness")]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
    }
}
