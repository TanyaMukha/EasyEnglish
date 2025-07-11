using EasyEnglish.Theme.Constants;

namespace EasyEnglish.Theme.Components.Typography;

public partial class H3 : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
}