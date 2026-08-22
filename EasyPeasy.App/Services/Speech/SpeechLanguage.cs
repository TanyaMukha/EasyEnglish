namespace EasyPeasy.App.Services.Speech;

/// <summary>A language/accent the text-to-speech subsystem can voice a chunk of text in.</summary>
public enum SpeechLanguage
{
    /// <summary>British English (matched to locales like en-GB, en-AU, en-IE).</summary>
    EnglishBritish,
    /// <summary>American English (matched to locales like en-US, en-CA).</summary>
    EnglishAmerican,
    /// <summary>Ukrainian (matched to uk-UA).</summary>
    Ukrainian
}
