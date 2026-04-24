using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace DoAn
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        DataScheme = "doan",
        DataHost = "restaurant")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OpenDeepLink(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
            OpenDeepLink(intent);
        }

        private static void OpenDeepLink(Intent? intent)
        {
            var link = intent?.Data?.ToString();
            if (string.IsNullOrWhiteSpace(link)) return;
            if (!Uri.TryCreate(link, UriKind.Absolute, out var uri)) return;

            Microsoft.Maui.Controls.Application.Current?.SendOnAppLinkRequestReceived(uri);
        }
    }
}
