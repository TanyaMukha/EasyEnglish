using EasyEnglish.Theme.Demo.Interfaces;
using EasyEnglish.Theme.Demo.Services;
using EasyEnglish.Theme.Extensions;
using Microsoft.Extensions.Logging;

namespace EasyEnglish.Theme.Demo
{
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
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            builder.Services.AddMauiBlazorWebView();

            // Add theme with demo configuration
            builder.Services.AddEasyEnglishTheme(options =>
            {
                options.EnableDebugMode = true;
                options.AutoInitializeResponsive = true;
            });

            // Add demo-specific services
            builder.Services.AddScoped<IDemoService, DemoService>();
            builder.Services.AddScoped<ICodeSnippetService, CodeSnippetService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
