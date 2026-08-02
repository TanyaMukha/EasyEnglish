namespace EasyEnglish.App.Components;

/// <summary>
/// Ids of layout sections — the "holes" a page can inject its own content into
/// via SectionContent, without knowing where in the layout it ends up.
/// Objects instead of string names: a typo is caught by the compiler, not at runtime.
/// </summary>
public static class AppSections
{
    /// <summary>
    /// Actions of the current page in the chrome row on narrow screens (right of the menu button).
    /// On desktop that row does not exist, so section content is not shown there.
    /// </summary>
    public static readonly object PageActions = new();

    /// <summary>
    /// Sticky bottom bar. Its content is usually born deep inside the page (for example, the
    /// drilling navigation buttons in DrillingPageLayout), so it travels here through a section
    /// rather than parameters — the components in between should not have to know about it.
    /// </summary>
    public static readonly object PageFooter = new();
}
