using EasyEnglish.Theme.Extensions;
using EasyEnglish.Theme.Interfaces;

namespace EasyEnglish.Theme.Services;

public class ColorService : IColorService
{
    private readonly IThemeConfiguration _config;

    private readonly Dictionary<string, string> _partOfSpeechColors = new()
    {
        ["noun"] = "color-noun",
        ["verb"] = "color-verb",
        ["adjective"] = "color-adjective",
        ["adverb"] = "color-adverb",
        ["preposition"] = "color-preposition",
        ["phrase"] = "color-phrase",
        ["phrasal_verb"] = "color-phrasal-verb",
        ["idiom"] = "color-idiom",
        ["pronoun"] = "color-pronoun",
        ["conjunction"] = "color-conjunction",
        ["interjection"] = "color-interjection",
        ["slang"] = "color-slang",
        ["abbreviation"] = "color-abbreviation",
        ["fixed_expression"] = "color-fixed-expression"
    };

    private readonly Dictionary<string, string> _levelColors = new()
    {
        ["A1"] = "color-level-a1",
        ["A2"] = "color-level-a2",
        ["B1"] = "color-level-b1",
        ["B2"] = "color-level-b2",
        ["C1"] = "color-level-c1",
        ["C2"] = "color-level-c2"
    };

    public ColorService(IThemeConfiguration config)
    {
        _config = config;
    }

    public string GetPartOfSpeechColor(string partOfSpeech)
    {
        var key = _partOfSpeechColors.GetValueOrDefault(partOfSpeech.ToLowerInvariant(), "color-primary");
        return _config.GetCssVariableValue(key);
    }

    public string GetLevelColor(string level)
    {
        var key = _levelColors.GetValueOrDefault(level.ToUpperInvariant(), "color-text-secondary");
        return _config.GetCssVariableValue(key);
    }

    public string GetSemanticColor(SemanticColorType type)
    {
        var key = type switch
        {
            SemanticColorType.Success => "color-success",
            SemanticColorType.Warning => "color-warning",
            SemanticColorType.Error => "color-error",
            SemanticColorType.Info => "color-info",
            SemanticColorType.Primary => "color-primary",
            SemanticColorType.Secondary => "color-secondary",
            SemanticColorType.Accent => "color-accent",
            _ => "color-primary"
        };

        return _config.GetCssVariableValue(key);
    }

    public string GetCssVariable(string variableName)
    {
        return _config.GetCssVariableValue(variableName);
    }

    public string GetContrastColor(string backgroundColor)
    {
        return IsLightColor(backgroundColor) ? "#000000" : "#FFFFFF";
    }

    public bool IsLightColor(string color)
    {
        if (color.StartsWith("#"))
        {
            var hex = color.Substring(1);
            if (hex.Length == 6)
            {
                var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                var brightness = (r * 299 + g * 587 + b * 114) / 1000;
                return brightness > 128;
            }
        }

        return false;
    }
}