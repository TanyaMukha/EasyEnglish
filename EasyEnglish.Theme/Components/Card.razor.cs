using EasyEnglish.Theme.Constants;
using EasyEnglish.Theme.Types;

namespace EasyEnglish.Theme.Components;

public partial class Card : ComponentBase
{
    [Parameter] public CardSize Size { get; set; } = CardSize.Medium;
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetCardClasses()
    {
        var classes = new List<string>();

        classes.Add(Size switch
        {
            CardSize.Small => ThemeConstants.CssClasses.CardSmall,
            CardSize.Medium => ThemeConstants.CssClasses.Card,
            CardSize.Large => ThemeConstants.CssClasses.CardLarge,
            _ => ThemeConstants.CssClasses.Card
        });

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }
}
