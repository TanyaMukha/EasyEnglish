using EasyEnglish.Theme.Interfaces;
using EasyEnglish.Theme.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Theme.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEasyEnglishTheme(
        this IServiceCollection services,
        Action<ThemeOptions>? configure = null)
    {
        // Configure theme options
        var options = new ThemeOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Register theme configuration
        services.AddSingleton<IThemeConfiguration>(provider =>
            new DefaultThemeConfiguration(provider.GetRequiredService<ThemeOptions>()));

        // Register theme services
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IResponsiveService, ResponsiveService>();
        services.AddScoped<IColorService, ColorService>();

        return services;
    }
}

public class ThemeOptions
{
    public bool EnableDebugMode { get; set; } = false;
    public Dictionary<string, string> CustomColors { get; set; } = new();
    public CustomBreakpoints? Breakpoints { get; set; }
    public string CssPrefix { get; set; } = "";
    public bool AutoInitializeResponsive { get; set; } = true;
}

public class CustomBreakpoints
{
    public int Mobile { get; set; } = 480;
    public int Tablet { get; set; } = 768;
    public int Desktop { get; set; } = 1024;
    public int LargeDesktop { get; set; } = 1440;
}

public interface IThemeConfiguration
{
    string GetCssVariableValue(string variableName);
    string GetColorByKey(string key);
    int GetSpacingValue(string size);
}

public class DefaultThemeConfiguration : IThemeConfiguration
{
    private readonly ThemeOptions _options;

    public DefaultThemeConfiguration(ThemeOptions options)
    {
        _options = options;
    }

    public string GetCssVariableValue(string variableName)
    {
        return _options.CustomColors.GetValueOrDefault(variableName, $"var(--{variableName})");
    }

    public string GetColorByKey(string key)
    {
        return _options.CustomColors.GetValueOrDefault(key, "");
    }

    public int GetSpacingValue(string size)
    {
        return size.ToLowerInvariant() switch
        {
            "xs" => 4,
            "sm" => 8,
            "md" => 16,
            "lg" => 24,
            "xl" => 32,
            "xxl" => 48,
            "xxxl" => 64,
            _ => 16
        };
    }
}