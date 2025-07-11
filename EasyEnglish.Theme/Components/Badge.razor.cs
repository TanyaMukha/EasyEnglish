using EasyEnglish.Theme.Constants;
using EasyEnglish.Theme.Interfaces;
using EasyEnglish.Theme.Services;
using EasyEnglish.Theme.Types;

namespace EasyEnglish.Theme.Components;

public partial class Badge : ComponentBase
{
    [Inject] public IThemeService ThemeService { get; set; } = default!;

    [Parameter] public BadgeVariant Variant { get; set; } = BadgeVariant.Default;
    [Parameter] public string? Level { get; set; }
    [Parameter] public string? PartOfSpeech { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetBadgeClasses()
    {
        var classes = new List<string> { ThemeConstants.CssClasses.Badge };

        if (Variant == BadgeVariant.Level && !string.IsNullOrEmpty(Level))
        {
            classes.Add(Level.ToUpperInvariant() switch
            {
                "A1" => ThemeConstants.CssClasses.LevelBadgeA1,
                "A2" => ThemeConstants.CssClasses.LevelBadgeA2,
                "B1" => ThemeConstants.CssClasses.LevelBadgeB1,
                "B2" => ThemeConstants.CssClasses.LevelBadgeB2,
                "C1" => ThemeConstants.CssClasses.LevelBadgeC1,
                "C2" => ThemeConstants.CssClasses.LevelBadgeC2,
                _ => ""
            });
        }

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }

    private string GetBadgeStyle()
    {
        if (!string.IsNullOrEmpty(PartOfSpeech))
        {
            var color = ThemeService.GetColorByPartOfSpeech(PartOfSpeech);
            return $"background-color: {color};";
        }

        return "";
    }
}
