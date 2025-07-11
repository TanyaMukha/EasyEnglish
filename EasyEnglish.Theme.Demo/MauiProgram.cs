using Microsoft.Extensions.Logging;
using EasyEnglish.Theme.Extensions;
using EasyEnglish.Theme.Demo.Services;
using EasyEnglish.Theme.Demo.Interfaces;

namespace EasyEnglish.Theme.Demo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add EasyEnglish Theme services
        builder.Services.AddEasyEnglishTheme(options =>
        {
            // Enable debug mode for development
            options.EnableDebugMode = true;

            // Enable automatic responsive initialization
            options.AutoInitializeResponsive = true;

            // Custom breakpoints if needed
            options.Breakpoints = new CustomBreakpoints
            {
                Mobile = 480,
                Tablet = 768,
                Desktop = 1200,
                LargeDesktop = 1600
            };

            // Custom colors for demo (optional)
            options.CustomColors.Add("demo-highlight", "#FFD700");
            options.CustomColors.Add("demo-secondary", "#FF6B35");
        });

        // Add demo-specific services
        builder.Services.AddScoped<IDemoService, DemoService>();
        builder.Services.AddScoped<ICodeSnippetService, CodeSnippetService>();

        // Add Blazor WebView services
        builder.Services.AddMauiBlazorWebView();

        // Add logging for debugging
#if DEBUG
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        return builder.Build();
    }
}