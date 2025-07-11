using EasyEnglish.Theme.Extensions;
using EasyEnglish.Theme.Interfaces;
using Microsoft.JSInterop;

namespace EasyEnglish.Theme.Services;

public class ResponsiveService : IResponsiveService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ThemeOptions _options;
    private DotNetObjectReference<ResponsiveService>? _dotNetRef;
    private bool _isInitialized = false;

    public event Action<ScreenSize>? ScreenSizeChanged;

    public ScreenSize CurrentScreenSize { get; private set; } = ScreenSize.Desktop;

    public bool IsMobile => CurrentScreenSize == ScreenSize.Mobile;
    public bool IsTablet => CurrentScreenSize == ScreenSize.Tablet;
    public bool IsDesktop => CurrentScreenSize == ScreenSize.Desktop;
    public bool IsLargeDesktop => CurrentScreenSize == ScreenSize.LargeDesktop;

    public ResponsiveService(IJSRuntime jsRuntime, ThemeOptions options)
    {
        _jsRuntime = jsRuntime;
        _options = options;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            var width = await _jsRuntime.InvokeAsync<int>("window.themeUtils.getScreenWidth");
            UpdateScreenSize(width);

            // Підписуємося на зміни розміру екрану
            await _jsRuntime.InvokeVoidAsync("window.themeUtils.subscribeToResize", _dotNetRef);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ResponsiveService initialization error: {ex.Message}");
            // Fallback to desktop
            CurrentScreenSize = ScreenSize.Desktop;
            _isInitialized = true;
        }
    }

    [JSInvokable]
    public void OnWindowResize(int width, int height)
    {
        var oldSize = CurrentScreenSize;
        UpdateScreenSize(width);

        if (oldSize != CurrentScreenSize)
        {
            ScreenSizeChanged?.Invoke(CurrentScreenSize);
        }
    }

    private void UpdateScreenSize(int width)
    {
        var breakpoints = _options.Breakpoints ?? new CustomBreakpoints();

        CurrentScreenSize = width switch
        {
            < 480 when width < breakpoints.Mobile => ScreenSize.Mobile,
            < 768 when width < breakpoints.Tablet => ScreenSize.Mobile,
            < 1024 when width < breakpoints.Desktop => ScreenSize.Tablet,
            < 1440 when width < breakpoints.LargeDesktop => ScreenSize.Desktop,
            _ => ScreenSize.LargeDesktop
        };
    }

    public string GetResponsiveClass(string mobileClass, string? tabletClass = null, string? desktopClass = null)
    {
        return CurrentScreenSize switch
        {
            ScreenSize.Mobile => mobileClass,
            ScreenSize.Tablet => tabletClass ?? mobileClass,
            ScreenSize.Desktop => desktopClass ?? tabletClass ?? mobileClass,
            ScreenSize.LargeDesktop => desktopClass ?? tabletClass ?? mobileClass,
            _ => mobileClass
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("window.themeUtils.unsubscribeFromResize");
            }
            catch
            {
                // Ignore errors during cleanup
            }

            _dotNetRef.Dispose();
        }
    }
}
