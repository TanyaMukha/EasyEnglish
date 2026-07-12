using System.Text.RegularExpressions;

namespace EasyEnglish.App.Services;

/// <summary>Removes bracketed annotations (e.g. "[formal]", "[plural]") from example/definition text.</summary>
public static class TextBracketsRemoverService
{
    private static readonly Regex BracketsPattern = new(@"(?<lead> ?)\[[^\[\]]*\](?<trail> ?)", RegexOptions.Compiled);

    /// <summary>
    /// Removes every <c>[...]</c> substring (brackets included) from <paramref name="text"/>.
    /// Collapses whichever single adjacent space would otherwise be left behind: prefers dropping
    /// the space right after the bracket, but falls back to dropping the space before it when
    /// there's no trailing space to remove and what follows isn't a letter or digit (end of
    /// string, punctuation, another bracket, etc.) — avoiding both a leftover double-space and an
    /// accidentally joined word.
    /// </summary>
    public static string RemoveBracketsText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var value = text;

        return BracketsPattern.Replace(value, match =>
        {
            var leadingSpace = match.Groups["lead"].Value;

            if (match.Groups["trail"].Length > 0)
                return leadingSpace;

            var nextIndex = match.Index + match.Length;
            var nextIsLetterOrDigit = nextIndex < value.Length && char.IsLetterOrDigit(value[nextIndex]);

            return nextIsLetterOrDigit ? leadingSpace : string.Empty;
        });
    }
}
