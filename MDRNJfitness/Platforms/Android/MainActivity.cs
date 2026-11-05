using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using AndroidX.Core.View;

namespace MDRNJfitness
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Draw behind the system bars on every Android version, with light icons for the dark UI
            EdgeToEdge.Enable(this);
            var bars = WindowCompat.GetInsetsController(Window!, Window!.DecorView);
            bars.AppearanceLightStatusBars = false;
            bars.AppearanceLightNavigationBars = false;

            // Pixel-styled native dialogs (alerts, pickers): see Resources/values/styles.xml
            Theme?.ApplyStyle(Resource.Style.PxDialogOverlay, true);
        }
    }
}
