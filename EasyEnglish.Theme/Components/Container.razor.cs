using EasyEnglish.Theme.Constants;
using EasyEnglish.Theme.Types;

namespace EasyEnglish.Theme.Components;

public partial class Container : ComponentBase
{
    [Parameter] public ContainerType Type { get; set; } = ContainerType.Default;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetContainerClasses()
    {
        var classes = new List<string>();

        classes.Add(Type switch
        {
            ContainerType.Default => ThemeConstants.CssClasses.Container,
            ContainerType.Padded => ThemeConstants.CssClasses.ContainerPadded,
            ContainerType.Screen => ThemeConstants.CssClasses.Screen,
            ContainerType.ScreenCentered => ThemeConstants.CssClasses.ScreenCentered,
            _ => ThemeConstants.CssClasses.Container
        });

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }
}