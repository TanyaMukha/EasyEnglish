namespace EasyEnglish.App.Services.SpeechRecognition;

/// <summary>
/// Prepares a word/form's text for pronunciation checking: strips bracketed annotations (via
/// <see cref="TextBracketsRemoverService"/>) and a single leading infinitive marker ("to") or
/// article ("a"/"an"/"the"), since learners typically don't voice these when just saying the
/// word on its own.
/// </summary>
public static class PronunciationTextNormalizer
{
    private static readonly string[] LeadingPrefixes = ["to ", "an ", "a ", "the "];

    /// <summary>Returns the text a learner is expected to say for <paramref name="text"/>, with brackets and a leading "to"/article stripped.</summary>
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
