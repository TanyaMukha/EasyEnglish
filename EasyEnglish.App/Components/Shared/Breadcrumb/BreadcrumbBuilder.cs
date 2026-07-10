namespace EasyEnglish.App.Components.Shared.Breadcrumb;

public class BreadcrumbBuilder
{
    private readonly List<Breadcrumb.BreadcrumbItem> _items = new();

    public static BreadcrumbBuilder Create() => new();

    public BreadcrumbBuilder WithHome(string url = "/", string icon = "house")
    {
        return WithLink("Головна", url, icon);
    }

    public BreadcrumbBuilder WithLink(string title, string url, string? icon = null)
    {
        _items.Add(new Breadcrumb.BreadcrumbItem
        {
            Title = title,
            Url = url,
            Icon = icon
        });
        return this;
    }

    public BreadcrumbBuilder WithActive(string title, string? icon = null)
    {
        _items.Add(new Breadcrumb.BreadcrumbItem
        {
            Title = title,
            Icon = icon,
            IsActive = true
        });
        return this;
    }

    public List<Breadcrumb.BreadcrumbItem> Build() => _items;
}
