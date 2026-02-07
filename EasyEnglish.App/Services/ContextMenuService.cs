namespace EasyEnglish.App.Services;

public class ContextMenuService
{
    public event Action<bool, double, double, string, Func<string, Task>>? OnMenuStateChanged;

    public void ShowMenu(double x, double y, string elementId, Func<string, Task> onValueChanged)
    {
        OnMenuStateChanged?.Invoke(true, x, y, elementId, onValueChanged);
    }

    public void HideMenu()
    {
        OnMenuStateChanged?.Invoke(false, 0, 0, string.Empty, _ => Task.CompletedTask);
    }
}