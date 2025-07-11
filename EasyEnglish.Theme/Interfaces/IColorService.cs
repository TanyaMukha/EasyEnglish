namespace EasyEnglish.Theme.Interfaces;

public interface IColorService
{
    string GetPartOfSpeechColor(string partOfSpeech);
    string GetLevelColor(string level);
    string GetSemanticColor(SemanticColorType type);
    string GetCssVariable(string variableName);
    string GetContrastColor(string backgroundColor);
    bool IsLightColor(string color);
}

public enum SemanticColorType
{
    Success,
    Warning,
    Error,
    Info,
    Primary,
    Secondary,
    Accent
}
