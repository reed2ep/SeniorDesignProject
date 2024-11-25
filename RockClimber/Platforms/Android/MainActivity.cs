using Android.App;
using Android.Content.PM;
using Android.OS;
using SQLite;
using System.IO;

namespace RockClimber
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static string DatabasePath // Set up saved paths local database location
        {
            get
            {
                var personalFolder = FileSystem.AppDataDirectory;
                var dbPath = Path.Combine(personalFolder, "savedPaths.db3"); // Database file name
                return dbPath;
            }
        }

    }
}
