#if ANDROID
using Android.Content;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace EasyPeasy.App.Services.Speech;

public static class AndroidVoiceInstallHelper
{
    public static void OpenTtsSettings()
    {
        var intent = new Intent("com.android.settings.TTS_SETTINGS");
        intent.SetFlags(ActivityFlags.NewTask);
        try
        {
            Android.App.Application.Context.StartActivity(intent);
        }
        catch
        {
            var fallback = new Intent(Android.Provider.Settings.ActionSettings);
            fallback.SetFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(fallback);
        }
    }

    public static void RequestLanguageInstall()
    {
        var intent = new Intent(TextToSpeech.Engine.ActionInstallTtsData);
        intent.SetFlags(ActivityFlags.NewTask);
        try
        {
            Android.App.Application.Context.StartActivity(intent);
        }
        catch
        {
            OpenTtsSettings();
        }
    }
}
#endif