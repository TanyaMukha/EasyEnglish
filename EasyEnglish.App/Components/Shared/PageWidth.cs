namespace EasyEnglish.App.Components.Shared;

/// <summary>
/// Width of the page column. Instead of every page writing its own max-width in CSS,
/// it picks one of these — so the set of values stays under control.
/// </summary>
public enum PageWidth
{
    /// <summary>800px — reading and running tests.</summary>
    Narrow,

    /// <summary>1000px — forms, detail pages, tooling pages.</summary>
    Medium,

    /// <summary>1100px — two-column exercise setup.</summary>
    Wide,

    /// <summary>1200px — lists and card grids.</summary>
    Full
}

public static class PageWidthExtensions
{
    public static string ToCssModifier(this PageWidth width) => width switch
    {
        PageWidth.Narrow => "narrow",
        PageWidth.Medium => "medium",
        PageWidth.Wide   => "wide",
        _                => "full"
    };
}
