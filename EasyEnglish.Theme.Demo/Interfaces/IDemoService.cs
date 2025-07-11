using EasyEnglish.Theme.Demo.Models;

namespace EasyEnglish.Theme.Demo.Interfaces;

public interface IDemoService
{
    List<ComponentDemoModel> GetComponentDemos();
    ComponentDemoModel? GetDemoById(string id);
    List<ColorPaletteModel> GetColorPalettes();
    List<TypographyExampleModel> GetTypographyExamples();
}