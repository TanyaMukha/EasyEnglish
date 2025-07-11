namespace EasyEnglish.Theme.Demo.Models;

public class ColorPaletteModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ColorItemModel> Colors { get; set; } = new();
}
