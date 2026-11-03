using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MDRNJfitness
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Pixel-styled native dialogs (alerts, pickers): see Resources/values/styles.xml
            Theme?.ApplyStyle(Resource.Style.PxDialogOverlay, true);
        }
    }
}
