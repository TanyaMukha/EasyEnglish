namespace EasyEnglish.Theme.Demo.Models;

public class ComponentDemoModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public List<ComponentVariantModel> Variants { get; set; } = new();
}
