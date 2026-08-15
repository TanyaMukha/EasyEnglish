using System.Text;

namespace EasyEnglish.App.Services;

/// <summary>
/// Renders a dictionary entry the way the learner should see it.
///
/// The authoring markers understood by <see cref="AnswerMatcher"/> are not shown as punctuation:
/// brackets around an optional part disappear and the part itself is dimmed instead, which says
/// "you may skip this" without the learner having to know the notation. Braces, being a purely
/// technical escape, vanish entirely.
///
/// Placeholders (sb / sth and their spellings) are dimmed for the same reason: they are slots to
/// be filled, not words to be learned, so they should not compete with the entry itself.
/// </summary>
public static class EntryTextRenderer
{
    /// <summary>Slot words: they stand for "somebody"/"something", not for themselves.</summary>
    private static readonly HashSet<string> Placeholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "sb", "sb's", "smb", "smb's", "somebody", "somebody's", "someone", "someone's",
        "sth", "sth's", "smth", "smth's", "something", "something's",
        "oneself",
    };

    /// <summary>
    /// HTML for <paramref name="entry"/>: optional parts wrapped in
    /// <c>&lt;span class="entry-optional"&gt;</c>, placeholders in
    /// <c>&lt;span class="entry-placeholder"&gt;</c>, markers removed, everything else escaped.
    /// </summary>
    public static string ToHtml(string? entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return string.Empty;

        var builder = new StringBuilder(entry.Length + 32);
        var plain   = new StringBuilder();

        void FlushPlain()
        {
            if (plain.Length == 0)
                return;

            builder.Append(RenderPlain(plain.ToString()));
            plain.Clear();
        }

        for (var i = 0; i < entry.Length; i++)
        {
            var ch = entry[i];

            if (ch is '{' or '}')
                continue;                     // technical marker, never shown

            if (ch == '[')
            {
                var close = entry.IndexOf(']', i + 1);

                if (close < 0)
                {
                    plain.Append(ch);         // unclosed bracket: leave it as plain text
                    continue;
                }

                FlushPlain();
                builder.Append("<span class=\"entry-optional\">")
                       .Append(Escape(entry[(i + 1)..close]))
                       .Append("</span>");

                i = close;
                continue;
            }

            plain.Append(ch);
        }

        FlushPlain();

        return builder.ToString();
    }

    /// <summary>Escapes a run of ordinary text, dimming any placeholder word inside it.</summary>
    private static string RenderPlain(string text)
    {
        var builder = new StringBuilder(text.Length + 16);
        var word    = new StringBuilder();

        void FlushWord()
        {
            if (word.Length == 0)
                return;

            var value = word.ToString();

            if (Placeholders.Contains(Trim(value)))
                builder.Append("<span class=\"entry-placeholder\">").Append(Escape(value)).Append("</span>");
            else
                builder.Append(Escape(value));

            word.Clear();
        }

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                FlushWord();
                builder.Append(ch);
                continue;
            }

            word.Append(ch);
        }

        FlushWord();

        return builder.ToString();
    }

    /// <summary>Strips punctuation around a word so "sth," or "(sb)" still reads as a placeholder.</summary>
    private static string Trim(string word) =>
        word.Trim('.', ',', ';', ':', '!', '?', '(', ')', '"', '“', '”', '…');

    private static string Escape(string text) => text
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;");
}
