namespace EasyEnglish.App.Services.Speech;

using System.Text.RegularExpressions;

internal record TextChunk(string Text, SpeechLanguage Language);

internal static class TextChunkParser
{
    private static readonly Regex InclusionPattern =
        new(@"\*\*(.+?)\*\*", RegexOptions.Compiled | RegexOptions.Singleline);

    public static IReadOnlyList<TextChunk> Parse(
        string text,
        SpeechLanguage primary,
        SpeechLanguage inclusion)
    {
        var result = new List<TextChunk>();
        var lastIndex = 0;

        foreach (Match match in InclusionPattern.Matches(text))
        {
            if (match.Index > lastIndex)
            {
                var before = text[lastIndex..match.Index].Trim();
                if (!string.IsNullOrEmpty(before))
                    result.Add(new TextChunk(before, primary));
            }

            var inner = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(inner))
                result.Add(new TextChunk(inner, inclusion));

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            var tail = text[lastIndex..].Trim();
            if (!string.IsNullOrEmpty(tail))
                result.Add(new TextChunk(tail, primary));
        }

        return result;
    }
}