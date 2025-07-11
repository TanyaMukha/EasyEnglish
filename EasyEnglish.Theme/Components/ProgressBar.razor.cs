using EasyEnglish.Theme.Constants;

namespace EasyEnglish.Theme.Components;

public partial class ProgressBar : ComponentBase
{
    [Parameter] public double Progress { get; set; }
    [Parameter] public double Min { get; set; } = 0;
    [Parameter] public double Max { get; set; } = 100;

    private double CalculatedProgress => ((Math.Max(Min, Math.Min(Max, Progress)) - Min) / (Max - Min)) * 100;
}
