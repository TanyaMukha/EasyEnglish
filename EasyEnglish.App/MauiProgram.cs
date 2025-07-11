﻿using EasyEnglish.Theme.Extensions;
using Microsoft.Extensions.Logging;

namespace EasyEnglish.App
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
                });

            builder.Services.AddMauiBlazorWebView();

            // Додаємо тему з кастомною конфігурацією
            builder.Services.AddEasyEnglishTheme(options =>
            {
                options.EnableDebugMode = true;
                options.AutoInitializeResponsive = true;
                options.CssPrefix = "easyenglish-";

                // Кастомні кольори
                options.CustomColors.Add("color-primary", "#FF6B35");
                options.CustomColors.Add("color-secondary", "#004E89");

                // Кастомні breakpoints
                options.Breakpoints = new CustomBreakpoints
                {
                    Mobile = 480,
                    Tablet = 768,
                    Desktop = 1200,
                    LargeDesktop = 1600
                };
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
