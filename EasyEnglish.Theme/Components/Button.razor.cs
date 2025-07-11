using EasyEnglish.Theme.Constants;
using EasyEnglish.Theme.Types;

namespace EasyEnglish.Theme.Components;

public partial class Button : ComponentBase
{
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Medium;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool PreventDefault { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetButtonClasses()
    {
        var classes = new List<string> { ThemeConstants.CssClasses.Button };

        classes.Add(Variant switch
        {
            ButtonVariant.Primary => ThemeConstants.CssClasses.ButtonPrimary,
            ButtonVariant.Secondary => ThemeConstants.CssClasses.ButtonSecondary,
            ButtonVariant.Accent => ThemeConstants.CssClasses.ButtonAccent,
            ButtonVariant.Success => ThemeConstants.CssClasses.ButtonSuccess,
            ButtonVariant.Warning => ThemeConstants.CssClasses.ButtonWarning,
            ButtonVariant.Error => ThemeConstants.CssClasses.ButtonError,
            ButtonVariant.Outline => ThemeConstants.CssClasses.ButtonOutline,
            ButtonVariant.Ghost => ThemeConstants.CssClasses.ButtonGhost,
            _ => ThemeConstants.CssClasses.ButtonPrimary
        });

        if (Disabled)
            classes.Add(ThemeConstants.CssClasses.ButtonDisabled);

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }
}