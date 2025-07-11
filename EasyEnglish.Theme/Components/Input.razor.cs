using EasyEnglish.Theme.Constants;

namespace EasyEnglish.Theme.Components;

public partial class Input : ComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool HasError { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public bool Multiline { get; set; }
    [Parameter] public int Rows { get; set; } = 3;
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetInputClasses()
    {
        var classes = new List<string>();

        if (Multiline)
            classes.Add(ThemeConstants.CssClasses.InputMultiline);
        else
            classes.Add(ThemeConstants.CssClasses.Input);

        if (HasError || !string.IsNullOrEmpty(ErrorMessage))
            classes.Add(ThemeConstants.CssClasses.InputError);

        if (Disabled)
            classes.Add(ThemeConstants.CssClasses.InputDisabled);

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }

    private async Task OnValueChanged(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
}