namespace EasyEnglish.Theme.Demo.Interfaces;

public interface ICodeSnippetService
{
    string GetRazorCode(string componentName, Dictionary<string, object> props);
    string GetCSharpCode(string componentName, Dictionary<string, object> props);
    string FormatCode(string code, string language);
}
