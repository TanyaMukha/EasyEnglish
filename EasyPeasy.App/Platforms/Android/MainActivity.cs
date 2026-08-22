using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Graphics;

namespace EasyPeasy;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | 
                          ConfigChanges.Orientation | 
                          ConfigChanges.UiMode | 
                          ConfigChanges.ScreenLayout | 
                          ConfigChanges.SmallestScreenSize | 
                          ConfigChanges.Density,
    WindowSoftInputMode = SoftInput.AdjustResize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Контент між панелями, не під ними
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window?.SetDecorFitsSystemWindows(true);
        }
        
        // ВИПРАВЛЕНО: Встановлюємо БІЛІ панелі
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            // Білий колір для обох панелей
            Window?.SetStatusBarColor(Android.Graphics.Color.White);
            Window?.SetNavigationBarColor(Android.Graphics.Color.White);
        }
        
        // Темний текст/іконки на білому фоні
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            var windowInsetsController = Window?.InsetsController;
            if (windowInsetsController != null && Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                // API 30+ (Android 11+)
                windowInsetsController.SetSystemBarsAppearance(
                    (int)WindowInsetsControllerAppearance.LightStatusBars | 
                    (int)WindowInsetsControllerAppearance.LightNavigationBars,
                    (int)WindowInsetsControllerAppearance.LightStatusBars | 
                    (int)WindowInsetsControllerAppearance.LightNavigationBars);
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // API 26-29 (Android 8-10)
                var flags = Window?.DecorView.SystemUiVisibility;
                flags |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                flags |= (StatusBarVisibility)SystemUiFlags.LightNavigationBar;
                Window!.DecorView.SystemUiVisibility = (StatusBarVisibility)flags;
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                // API 23-25 (Android 6-7) - тільки status bar
                var flags = Window?.DecorView.SystemUiVisibility;
                flags |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                Window!.DecorView.SystemUiVisibility = (StatusBarVisibility)flags;
            }
        }
    }
}
