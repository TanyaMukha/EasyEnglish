using System.Text;

namespace EasyPeasy.App.Services;

/// <summary>
/// Decides whether a learner's typed answer matches the expected word or phrase.
///
/// A dictionary entry is not a single string the learner must reproduce character by character —
/// it carries optional parts, and every combination of them is a legitimate answer.
///
/// The permission is one-way: the learner may leave out what the entry marks as optional, but may
/// never add a word the entry does not have. A word does not automatically take an article, so
/// accepting "a cat" for the entry "cat" would teach the wrong thing.
///
/// <list type="bullet">
///   <item>indefinite articles ("a", "an") present in the entry may be typed or left out;</item>
///   <item>a leading infinitive marker ("to", and likewise a leading "the") may be typed or left out;</item>
///   <item>anything in square brackets is optional, independently of every other bracket group —
///         two groups therefore accept four answers, three groups eight, and so on;</item>
///   <item>placeholders may be written in full or abbreviated: sb / somebody / someone,
///         sth / something (also smb / smth, and the possessive forms);</item>
///   <item>a slash separates equivalent wordings — typing any one of them is enough.</item>
/// </list>
///
/// Rather than expanding all those combinations (which grows as 2^n), the expected text is parsed
/// into segments — required, optional, or literal — and matched against the learner's tokens with
/// backtracking.
///
/// Authoring conventions in the expected text:
/// <list type="bullet">
///   <item><c>[...]</c> — the whole group is optional: <c>look [at] sb</c>.</item>
///   <item><c>{...}</c> — literal: nothing inside is treated as optional. This is the escape hatch
///         for a "to" that is not an infinitive marker (<c>{to} date</c>) or an article that is
///         genuinely part of the entry (<c>{a} priori</c>).</item>
///   <item><c>/</c> — equivalent wordings. A slash with a space next to it separates whole
///         alternatives (<c>configuration / config</c>, <c>to configure / to set up</c>); a slash
///         glued between two words is an alternative for that position only
///         (<c>look at sb/sth</c> = "look at sb" or "look at sth").</item>
/// </list>
/// </summary>
public static class AnswerMatcher
{
    /// <summary>Placeholder spellings that mean the same thing.</summary>
    private static readonly string[][] SynonymGroups =
    [
        ["sb", "smb", "somebody", "someone"],
        ["sb's", "smb's", "somebody's", "someone's"],
        ["sth", "smth", "something"],
        ["sth's", "smth's", "something's"],
        ["oneself", "yourself"],
    ];

    /// <summary>Articles that stay optional wherever they appear.</summary>
    private static readonly string[] OptionalAnywhere = ["a", "an"];

    /// <summary>
    /// Words that are optional only as the first token: "to" is an infinitive marker there
    /// ("to look at sb"), while further along it is part of the phrase ("go to school").
    /// The definite article follows the same rule for the same reason ("the same" vs "in the end").
    /// </summary>
    private static readonly string[] OptionalWhenLeading = ["to", "the"];

    /// <summary>True when <paramref name="userInput"/> is an acceptable answer for <paramref name="expected"/>.</summary>
    public static bool Matches(string? expected, string? userInput)
    {
        var typed = Tokenize(userInput);

        foreach (var alternative in SplitAlternatives(expected))
        {
            var segments = Parse(alternative);

            // An entry with no required content accepts only an empty answer.
            if (segments.Count == 0)
            {
                if (typed.Count == 0)
                    return true;

                continue;
            }

            if (MatchFrom(segments, 0, typed, 0))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Splits the entry on slashes that separate whole wordings — a slash with whitespace on either
    /// side, outside any bracket group. A slash glued between two words ("sb/sth") is left alone:
    /// that one belongs to a single position and is handled while parsing the word.
    /// </summary>
    private static List<string> SplitAlternatives(string? expected)
    {
        var alternatives = new List<string>();

        if (string.IsNullOrWhiteSpace(expected))
        {
            alternatives.Add(string.Empty);
            return alternatives;
        }

        var depth = 0;
        var start = 0;

        for (var i = 0; i < expected.Length; i++)
        {
            var ch = expected[i];

            if (ch is '[' or '{') depth++;
            else if (ch is ']' or '}') depth = Math.Max(0, depth - 1);
            else if (ch == '/' && depth == 0 && IsPhraseSeparator(expected, i))
            {
                alternatives.Add(expected[start..i]);
                start = i + 1;
            }
        }

        alternatives.Add(expected[start..]);

        return alternatives;
    }

    private static bool IsPhraseSeparator(string text, int slashIndex)
    {
        var spaceBefore = slashIndex > 0 && char.IsWhiteSpace(text[slashIndex - 1]);
        var spaceAfter  = slashIndex + 1 < text.Length && char.IsWhiteSpace(text[slashIndex + 1]);

        return spaceBefore || spaceAfter;
    }

    /// <summary>
    /// The entry as the learner should see it: <c>{literal}</c> markers removed, <c>[optional]</c>
    /// ones kept — the brackets tell the learner that part may be left out, the braces are an
    /// authoring detail that means nothing to them.
    /// </summary>
    public static string StripLiteralMarkers(string? expected)
    {
        if (string.IsNullOrWhiteSpace(expected))
            return string.Empty;

        return Tidy(expected.Replace("{", string.Empty).Replace("}", string.Empty));
    }

    /// <summary>
    /// The expected answer with every authoring marker removed and all optional content kept —
    /// one plain phrase, for places that need the full form without any notation.
    /// </summary>
    public static string ToDisplayForm(string? expected)
    {
        if (string.IsNullOrWhiteSpace(expected))
            return string.Empty;

        var builder = new StringBuilder(expected.Length);

        foreach (var ch in expected)
        {
            if (ch is '[' or ']' or '{' or '}')
                continue;

            builder.Append(ch);
        }

        return Tidy(builder.ToString());
    }

    private static string Tidy(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    // ── Matching ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Walks the expected segments against the typed tokens. Optional segments are tried both ways,
    /// which is what makes every combination of brackets/articles acceptable without enumerating them.
    /// </summary>
    private static bool MatchFrom(List<Segment> segments, int segmentIndex, List<string> typed, int typedIndex)
    {
        if (segmentIndex == segments.Count)
            return typedIndex == typed.Count;

        var segment = segments[segmentIndex];

        // Take the segment: every token in it must line up with the typed tokens.
        if (typedIndex + segment.Tokens.Count <= typed.Count)
        {
            var allMatch = true;

            for (var i = 0; i < segment.Tokens.Count; i++)
            {
                if (!TokenMatches(segment.Tokens[i], typed[typedIndex + i], segment.IsLiteral))
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch && MatchFrom(segments, segmentIndex + 1, typed, typedIndex + segment.Tokens.Count))
                return true;
        }

        // Or skip it, when the learner is allowed to leave it out.
        //
        // Only in that direction: leaving out what the entry marks optional is fine, adding a word
        // the entry does not have is not. A word is not automatically usable with an article, so
        // accepting "a cat" for the entry "cat" would teach the wrong thing.
        return segment.IsOptional && MatchFrom(segments, segmentIndex + 1, typed, typedIndex);
    }

    /// <summary>
    /// One position of the entry against one typed word. The position may accept several spellings:
    /// the ones the author wrote with a slash, plus the placeholder synonyms.
    /// </summary>
    private static bool TokenMatches(string[] expectedAlternatives, string typed, bool isLiteral)
    {
        foreach (var expected in expectedAlternatives)
        {
            if (string.Equals(expected, typed, StringComparison.Ordinal))
                return true;

            if (isLiteral)
                continue;

            foreach (var group in SynonymGroups)
            {
                if (Array.IndexOf(group, expected) >= 0 && Array.IndexOf(group, typed) >= 0)
                    return true;
            }
        }

        return false;
    }

    // ── Parsing the expected text ─────────────────────────────────────────────

    private static List<Segment> Parse(string? expected)
    {
        var segments = new List<Segment>();

        if (string.IsNullOrWhiteSpace(expected))
            return segments;

        var index = 0;

        while (index < expected.Length)
        {
            var ch = expected[index];

            if (char.IsWhiteSpace(ch))
            {
                index++;
                continue;
            }

            if (ch is '[' or '{')
            {
                var close = expected.IndexOf(ch == '[' ? ']' : '}', index + 1);

                // An unclosed marker is treated as plain words rather than swallowing the rest.
                if (close < 0)
                {
                    foreach (var word in expected[index..].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                        AddWord(segments, word, isLeading: segments.Count == 0);

                    break;
                }

                var inner  = expected[(index + 1)..close];
                var tokens = TokenizeExpected(inner);

                if (tokens.Count > 0)
                {
                    segments.Add(new Segment(
                        Tokens:     tokens,
                        IsOptional: ch == '[',
                        IsLiteral:  ch == '{'));
                }

                index = close + 1;
                continue;
            }

            var end = index;
            while (end < expected.Length && !char.IsWhiteSpace(expected[end]) && expected[end] is not ('[' or '{'))
                end++;

            AddWord(segments, expected[index..end], isLeading: segments.Count == 0);
            index = end;
        }

        return segments;
    }

    private static void AddWord(List<Segment> segments, string raw, bool isLeading)
    {
        var alternatives = SplitWordAlternatives(raw);

        if (alternatives.Length == 0)
            return;

        // "a/an" stays optional; a position is only optional when every spelling of it is.
        var isOptional = alternatives.All(word =>
            Array.IndexOf(OptionalAnywhere, word) >= 0
            || (isLeading && Array.IndexOf(OptionalWhenLeading, word) >= 0));

        segments.Add(new Segment([alternatives], IsOptional: isOptional, IsLiteral: false));
    }

    /// <summary>Turns "at / on" into "at/on" so both spellings of a one-position choice parse alike.</summary>
    private static string GlueSpacedSlashes(string text)
    {
        var builder = new StringBuilder(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                var next = i;
                while (next < text.Length && char.IsWhiteSpace(text[next]))
                    next++;

                // Drop the whitespace only when it hugs a slash on either side.
                var slashAhead  = next < text.Length && text[next] == '/';
                var slashBehind = builder.Length > 0 && builder[^1] == '/';

                if (slashAhead || slashBehind)
                {
                    i = next - 1;
                    continue;
                }

                builder.Append(' ');
                i = next - 1;
                continue;
            }

            builder.Append(text[i]);
        }

        return builder.ToString();
    }

    /// <summary>Splits a single word on glued slashes: "sb/sth" accepts either spelling here.</summary>
    private static string[] SplitWordAlternatives(string raw) =>
        raw.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .Where(word => word.Length > 0)
            .ToArray();

    // ── Normalization ─────────────────────────────────────────────────────────

    /// <summary>Tokenizes a piece of the entry: every position keeps its slash-separated spellings.</summary>
    private static List<string[]> TokenizeExpected(string? text)
    {
        var tokens = new List<string[]>();

        if (string.IsNullOrWhiteSpace(text))
            return tokens;

        // Inside a group there is nothing to split into whole alternatives, so "[at / on]" means
        // the same as "[at/on]" — glue the spaced form before tokenizing.
        foreach (var raw in GlueSpacedSlashes(text).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var alternatives = SplitWordAlternatives(raw);

            if (alternatives.Length > 0)
                tokens.Add(alternatives);
        }

        return tokens;
    }

    private static List<string> Tokenize(string? text)
    {
        var tokens = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return tokens;

        foreach (var raw in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var token = Normalize(raw);

            if (token.Length > 0)
                tokens.Add(token);
        }

        return tokens;
    }

    /// <summary>
    /// Lower-cases a token, unifies apostrophes and drops surrounding punctuation, so that
    /// "Somebody's," and "somebody’s" compare equal.
    /// </summary>
    private static string Normalize(string raw)
    {
        var builder = new StringBuilder(raw.Length);

        foreach (var ch in raw.ToLowerInvariant())
        {
            if (ch is '’' or '‘' or '`' or '´')
            {
                builder.Append('\'');
                continue;
            }

            if (char.IsLetterOrDigit(ch) || ch is '\'' or '-')
                builder.Append(ch);
        }

        return builder.ToString().Trim('\'', '-');
    }

    /// <summary>
    /// One piece of the expected answer: a single word, or the whole content of a bracket group.
    /// </summary>
    /// <param name="Tokens">Positions this segment expects, in order; each holds its accepted spellings.</param>
    /// <param name="IsOptional">Whether the learner may leave the whole segment out.</param>
    /// <param name="IsLiteral">Whether synonym spellings are rejected inside this segment.</param>
    private sealed record Segment(List<string[]> Tokens, bool IsOptional, bool IsLiteral);
}
