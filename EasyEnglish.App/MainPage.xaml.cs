namespace EasyEnglish.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
        }

        private void OnBlazorWebViewInitialized(object? sender, Microsoft.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs e)
        {
#if WINDOWS
            // Відкриття нативного діалогу вибору файлу (InputFile) на Windows іноді
            // валить рендер-процес WebView2, і без цього обробника весь застосунок
            // завершується без жодного логу. Тут ми хоча б фіксуємо збій — сама
            // сторінка перезавантажиться автоматично, якщо AreBrowserAcceleratorKeysEnabled
            // дозволяє відновлення; інакше користувачу знадобиться перезапустити застосунок.
            if (e.WebView is Microsoft.UI.Xaml.Controls.WebView2 webView2)
            {
                webView2.CoreWebView2Initialized += (sender, _) =>
                {
                    ((Microsoft.UI.Xaml.Controls.WebView2)sender).CoreWebView2.ProcessFailed += (_, failedArgs) =>
                    {
                        EasyEnglish.App.Diagnostics.CrashLogger.Log(
                            $"WebView2.ProcessFailed: Kind={failedArgs.ProcessFailedKind}, ExitCode={failedArgs.ExitCode}, Description={failedArgs.ProcessDescription}",
                            null);
                    };
                };
            }
#endif
        }
    }
}
