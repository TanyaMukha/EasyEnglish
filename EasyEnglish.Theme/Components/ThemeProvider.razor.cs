using EasyEnglish.Theme.Constants;
using EasyEnglish.Theme.Interfaces;

namespace EasyEnglish.Theme.Components;

public partial class ThemeProvider : ComponentBase, IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string LoadingMessage { get; set; } = "Initializing theme...";

    [Inject] public IThemeService ThemeService { get; set; } = default!;
    [Inject] public IResponsiveService ResponsiveService { get; set; } = default!;
    [Inject] public IColorService ColorService { get; set; } = default!;

    public bool IsInitialized { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await ThemeService.InitializeAsync();
                await ResponsiveService.InitializeAsync();
                IsInitialized = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme initialization error: {ex.Message}");
                IsInitialized = true; // Continue with defaults
                StateHasChanged();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (ResponsiveService is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}