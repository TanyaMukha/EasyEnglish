using System.Text.RegularExpressions;

namespace EasyEnglish.App.Services;

/// <summary>
/// Parses and renders example sentences that mix a small markdown subset with an optional
/// "hidden text" span (the part a flashcard blurs out until the learner reveals it).
/// </summary>
public static class ExampleMarkdownService
{
    /// <summary>Which markdown marker, if any, denotes the hidden/blurred span in a sentence.</summary>
    public enum HiddenTextMarker
    {
        /// <summary>No hidden text — the whole sentence renders normally.</summary>
        None,
        /// <summary><c>**text**</c></summary>
        Bold,
        /// <summary><c>__text__</c></summary>
        Italic,
        /// <summary><c>***text***</c></summary>
        BoldItalic,
        /// <summary><c>`text`</c></summary>
        Code
    }

    private static readonly Dictionary<HiddenTextMarker, string> MarkerPatterns = new()
    {
        { HiddenTextMarker.Bold, @"\*\*(.+?)\*\*" },
        { HiddenTextMarker.Italic, @"__(.+?)__" },
        { HiddenTextMarker.BoldItalic, @"\*\*\*(.+?)\*\*\*" },
        { HiddenTextMarker.Code, @"`(.+?)`" }
    };

    /// <summary>
    /// Splits <paramref name="sentence"/> at the first occurrence of <paramref name="marker"/> into
    /// the text before it, the hidden text itself (marker stripped), and the text after it. Only
    /// the first match is considered — see <see cref="ParseSegments"/> for multi-occurrence support.
    /// Returns <c>(sentence, "", "")</c> if there's no match.
    /// </summary>
    public static (string beforeHidden, string hiddenText, string afterHidden) ParseHiddenText(
        string sentence,
        HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return (sentence, "", "");

        var pattern = MarkerPatterns[marker];
        var match = Regex.Match(sentence, pattern);

        if (match.Success)
        {
            var beforeIndex = match.Index;
            var afterIndex = match.Index + match.Length;

            var before = sentence.Substring(0, beforeIndex);
            var hidden = match.Groups[1].Value;
            var after = sentence.Substring(afterIndex);

            return (before, hidden, after);
        }

        return (sentence, "", "");
    }

    /// <summary>Returns whether <paramref name="sentence"/> contains at least one occurrence of <paramref name="marker"/>.</summary>
    public static bool HasHiddenText(string sentence, HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return false;

        var pattern = MarkerPatterns[marker];
        return Regex.IsMatch(sentence, pattern);
    }

    /// <summary>
    /// Renders the small markdown subset (bold-italic, bold, italic, code, links, newlines) to
    /// HTML. When <paramref name="hiddenMarker"/> is not <see cref="HiddenTextMarker.None"/>, that
    /// marker's pattern is skipped here — the caller is expected to have already extracted/rendered
    /// the hidden span separately (see <see cref="RenderMarkdownWithHidden"/>), so this doesn't
    /// double-render it.
    /// </summary>
    public static string RenderMarkdown(
        string text,
        HiddenTextMarker hiddenMarker = HiddenTextMarker.None)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var html = text;

        // Bold-italic: ***text*** (must run before the plain Bold/Italic patterns below.)
        if (hiddenMarker != HiddenTextMarker.BoldItalic)
        {
            html = Regex.Replace(html, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
        }

        // Bold: **text**
        if (hiddenMarker != HiddenTextMarker.Bold)
        {
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        }

        // Italic: __text__
        if (hiddenMarker != HiddenTextMarker.Italic)
        {
            html = Regex.Replace(html, @"__(.+?)__", "<em>$1</em>");
        }

        // Code: `text`
        if (hiddenMarker != HiddenTextMarker.Code)
        {
            html = Regex.Replace(html, @"`(.+?)`", "<code>$1</code>");
        }

        // Link: [text](url)
        html = Regex.Replace(html, @"\[(.+?)\]\((.+?)\)", "<a href=\"$2\">$1</a>");

        // Newlines
        html = html.Replace("\n", "<br />");

        return html;
    }

    /// <summary>
    /// Like <see cref="RenderMarkdown"/>, but also renders the hidden span itself as a
    /// <c>&lt;span class="hidden-text revealed"&gt;</c> (when <paramref name="showHidden"/> is
    /// <c>true</c>) or <c>&lt;span class="hidden-text blurred"&gt;</c> (otherwise) — the CSS classes
    /// the flashcard UI hooks into to show/blur the text.
    /// </summary>
    public static string RenderMarkdownWithHidden(
        string sentence,
        bool showHidden = false,
        HiddenTextMarker hiddenMarker = HiddenTextMarker.Bold)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return string.Empty;

        var html = sentence;

        // Render the hidden span first, before the rest of the markdown.
        if (hiddenMarker != HiddenTextMarker.None && MarkerPatterns.ContainsKey(hiddenMarker))
        {
            var pattern = MarkerPatterns[hiddenMarker];

            if (showHidden)
            {
                html = Regex.Replace(
                    html,
                    pattern,
                    "<span class='hidden-text revealed'>$1</span>"
                );
            }
            else
            {
                html = Regex.Replace(
                    html,
                    pattern,
                    "<span class='hidden-text blurred'>$1</span>"
                );
            }
        }

        // Then the rest of the markdown.
        html = RenderMarkdown(html, hiddenMarker);

        return html;
    }

    /// <summary>Strips every recognized markdown marker (bold-italic, bold, italic, code), leaving plain text.</summary>
    public static string StripMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var clean = text;

        clean = Regex.Replace(clean, @"\*\*\*(.+?)\*\*\*", "$1"); // ***text***
        clean = Regex.Replace(clean, @"\*\*(.+?)\*\*", "$1");     // **text**
        clean = Regex.Replace(clean, @"__(.+?)__", "$1");         // __text__
        clean = Regex.Replace(clean, @"`(.+?)`", "$1");           // `text`

        return clean;
    }

    /// <summary>Returns just the hidden span's text (marker stripped), or <c>""</c> if there's no match.</summary>
    public static string GetHiddenTextOnly(string sentence, HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return "";

        var pattern = MarkerPatterns[marker];
        var match = Regex.Match(sentence, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }

    /// <summary>
    /// Compares a learner's manual-input answer against the correct one. Delegates to
    /// <see cref="AnswerMatcher"/>, so the hidden word in an example accepts the same variations as
    /// any other typed answer (optional articles, bracketed parts, sb/sth spellings).
    /// </summary>
    public static bool CheckAnswer(string userAnswer, string correctAnswer) =>
        AnswerMatcher.Matches(correctAnswer, userAnswer);

    /// <summary>
    /// Splits <paramref name="sentence"/> into an ordered sequence of plain-text and hidden
    /// segments, supporting any number of marker occurrences (unlike <see cref="ParseHiddenText"/>,
    /// which only sees the first). Used by cards that reveal several blurred words within one
    /// sentence.
    /// </summary>
    public static List<(bool IsHidden, string Text)> ParseSegments(
        string sentence,
        HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        var segments = new List<(bool IsHidden, string Text)>();

        if (string.IsNullOrEmpty(sentence))
            return segments;

        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
        {
            segments.Add((false, sentence));
            return segments;
        }

        var pattern = MarkerPatterns[marker];
        var lastIndex = 0;

        foreach (Match match in Regex.Matches(sentence, pattern))
        {
            if (match.Index > lastIndex)
                segments.Add((false, sentence[lastIndex..match.Index]));

            segments.Add((true, match.Groups[1].Value));
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < sentence.Length)
            segments.Add((false, sentence[lastIndex..]));

        return segments;
    }
}