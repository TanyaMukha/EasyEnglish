namespace EasyEnglish.Theme.Interfaces;

public interface IResponsiveService
{
    event Action<ScreenSize>? ScreenSizeChanged;
    ScreenSize CurrentScreenSize { get; }
    bool IsMobile { get; }
    bool IsTablet { get; }
    bool IsDesktop { get; }
    bool IsLargeDesktop { get; }
    Task InitializeAsync();
    string GetResponsiveClass(string mobileClass, string? tabletClass = null, string? desktopClass = null);
}

public enum ScreenSize
{
    Mobile,
    Tablet,
    Desktop,
    LargeDesktop
}
