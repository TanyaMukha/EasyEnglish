using EasyEnglish.Theme.Interfaces;
using Microsoft.JSInterop;

namespace EasyEnglish.Theme.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private int _screenWidth = 1024; // Default assumption

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsTablet => _screenWidth >= 768 && _screenWidth < 1024;
    public bool IsDesktop => _screenWidth >= 1024;

    private readonly Dictionary<string, string> _partOfSpeechColors = new()
    {
        ["noun"] = "var(--color-noun)",
        ["verb"] = "var(--color-verb)",
        ["adjective"] = "var(--color-adjective)",
        ["adverb"] = "var(--color-adverb)",
        ["preposition"] = "var(--color-preposition)",
        ["phrase"] = "var(--color-phrase)",
        ["phrasal_verb"] = "var(--color-phrasal-verb)",
        ["idiom"] = "var(--color-idiom)",
        ["pronoun"] = "var(--color-pronoun)",
        ["conjunction"] = "var(--color-conjunction)",
        ["interjection"] = "var(--color-interjection)",
        ["slang"] = "var(--color-slang)",
        ["abbreviation"] = "var(--color-abbreviation)",
        ["fixed_expression"] = "var(--color-fixed-expression)"
    };

    private readonly Dictionary<string, string> _levelColors = new()
    {
        ["A1"] = "var(--color-level-a1)",
        ["A2"] = "var(--color-level-a2)",
        ["B1"] = "var(--color-level-b1)",
        ["B2"] = "var(--color-level-b2)",
        ["C1"] = "var(--color-level-c1)",
        ["C2"] = "var(--color-level-c2)"
    };

    private readonly Dictionary<SpacingSize, int> _spacingValues = new()
    {
        [SpacingSize.Xs] = 4,
        [SpacingSize.Sm] = 8,
        [SpacingSize.Md] = 16,
        [SpacingSize.Lg] = 24,
        [SpacingSize.Xl] = 32,
        [SpacingSize.Xxl] = 48,
        [SpacingSize.Xxxl] = 64
    };

    private readonly Dictionary<ThemeFontSize, int> _fontSizes = new()
    {
        [ThemeFontSize.Xs] = 12,
        [ThemeFontSize.Sm] = 14,
        [ThemeFontSize.Base] = 16,
        [ThemeFontSize.Lg] = 18,
        [ThemeFontSize.Xl] = 20,
        [ThemeFontSize.Xl2] = 24,
        [ThemeFontSize.Xl3] = 30,
        [ThemeFontSize.Xl4] = 36,
        [ThemeFontSize.Xl5] = 48
    };

    public string GetColorByPartOfSpeech(string partOfSpeech)
    {
        return _partOfSpeechColors.GetValueOrDefault(partOfSpeech.ToLowerInvariant(), "var(--color-primary)");
    }

    public string GetLevelColor(string level)
    {
        return _levelColors.GetValueOrDefault(level.ToUpperInvariant(), "var(--color-text-secondary)");
    }

    public string GetResponsiveClass(string baseClass, string? tabletClass = null, string? desktopClass = null)
    {
        var classes = new List<string> { baseClass };

        if (!string.IsNullOrEmpty(tabletClass))
            classes.Add($"tablet:{tabletClass}");

        if (!string.IsNullOrEmpty(desktopClass))
            classes.Add($"desktop:{desktopClass}");

        return string.Join(" ", classes);
    }

    public int GetSpacing(SpacingSize size)
    {
        return _spacingValues[size];
    }

    public int GetFontSize(ThemeFontSize size)
    {
        return _fontSizes[size];
    }

    public string GetShadowClass(ShadowSize size)
    {
        return size switch
        {
            ShadowSize.Sm => "shadow-sm",
            ShadowSize.Md => "shadow-md",
            ShadowSize.Lg => "shadow-lg",
            ShadowSize.Xl => "shadow-xl",
            _ => "shadow-md"
        };
    }

    public async Task InitializeAsync()
    {
        try
        {
            _screenWidth = await _jsRuntime.InvokeAsync<int>("eval", "window.innerWidth");
        }
        catch
        {
            // Fallback to default if JS is not available
            _screenWidth = 1024;
        }
    }
}