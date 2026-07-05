using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EasyEnglish.App.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        // Без цього обробника необроблений виняток на UI-потоці (наприклад, з COM-викликів
        // нативного діалогу вибору файлу через WebView2) завершує весь процес без жодного
        // логу. Позначаємо e.Handled = true, щоб застосунок продовжив роботу, і пишемо деталі
        // винятку у файл — це дає змогу діагностувати причину, якщо збій повториться.
        private void OnUnhandledException(object? sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            EasyEnglish.App.Diagnostics.CrashLogger.Log("WinUI.UnhandledException", e.Exception);
            e.Handled = true;
        }
    }

}
