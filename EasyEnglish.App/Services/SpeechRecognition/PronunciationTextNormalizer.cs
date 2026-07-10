namespace EasyEnglish.App.Services.SpeechRecognition;

/// <summary>
/// Готує текст слова/форми для перевірки вимови: прибирає вміст у квадратних дужках
/// та початковий інфінітивний маркер "to"/артикль "a"/"an"/"the", які зазвичай не
/// вимовляють окремо, коли просто називають слово.
/// </summary>
public static class PronunciationTextNormalizer
{
    private static readonly string[] LeadingPrefixes = ["to ", "an ", "a ", "the "];

    public static string PrepareExpectedText(string? text)
    {
        var value = TextBracketsRemoverService.RemoveBracketsText(text).Trim();

        foreach (var prefix in LeadingPrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return value[prefix.Length..].TrimStart();
        }

        return value;
    }
}
