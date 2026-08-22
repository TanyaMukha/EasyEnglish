namespace EasyPeasy.App.Services.Speech;

using System.Text.RegularExpressions;

/// <summary>One parsed piece of a <see cref="SpeechSegment"/>'s text, tagged with the language it should be voiced in.</summary>
internal record TextChunk(string Text, SpeechLanguage Language);

/// <summary>
/// Splits a <see cref="SpeechSegment"/>'s text at <c>**double asterisks**</c> markers into an
/// ordered sequence of <see cref="TextChunk"/>s, alternating between the segment's primary
/// language and its inclusion language for whatever falls inside the markers. Empty chunks
/// (after trimming) are dropped rather than emitted.
/// </summary>
internal static class TextChunkParser
{
    private static readonly Regex InclusionPattern =
        new(@"\*\*(.+?)\*\*", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Parses <paramref name="text"/> into chunks, voicing text outside <c>**markers**</c> in
    /// <paramref name="primary"/> and text inside them in <paramref name="inclusion"/>.
    /// </summary>
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