using EasyEnglish.Theme.Demo.Interfaces;

namespace EasyEnglish.Theme.Demo.Services;

public class CodeSnippetService : ICodeSnippetService
{
    public string GetRazorCode(string componentName, Dictionary<string, object> props)
    {
        var propStrings = props.Select(p => $"{p.Key}=\"{p.Value}\"");
        var propsString = props.Any() ? " " + string.Join(" ", propStrings) : "";
        return $"<{componentName}{propsString}>{componentName} Content</{componentName}>";
    }

    public string GetCSharpCode(string componentName, Dictionary<string, object> props)
    {
        if (!props.Any())
        {
            return $@"// Create {componentName} programmatically
var {componentName.ToLower()} = new {componentName}();";
        }

        var propAssignments = props.Select(p => $"    {p.Key} = {FormatValue(p.Value)},");
        return $@"// Create {componentName} programmatically
var {componentName.ToLower()} = new {componentName}
{{
{string.Join(Environment.NewLine, propAssignments)}
}};";
    }

    public string FormatCode(string code, string language)
    {
        return code.Trim();
    }

    private string FormatValue(object value)
    {
        return value switch
        {
            string s => $"\"{s}\"",
            bool b => b.ToString().ToLowerInvariant(),
            int i => i.ToString(),
            double d => d.ToString("F2"),
            float f => f.ToString("F2") + "f",
            null => "null",
            _ => $"\"{value}\""
        };
    }
}
