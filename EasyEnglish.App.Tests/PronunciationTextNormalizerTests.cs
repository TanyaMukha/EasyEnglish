using EasyEnglish.App.Services.SpeechRecognition;

namespace EasyEnglish.App.Tests;

public class PronunciationTextNormalizerTests
{
    [Theory]
    [InlineData("to run", "run")]
    [InlineData("an apple", "apple")]
    [InlineData("a cat", "cat")]
    [InlineData("the dog", "dog")]
    [InlineData("The Big Cat", "Big Cat")]
    [InlineData("run", "run")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void PrepareExpectedText_StripsLeadingArticleOrInfinitiveMarker(string? input, string expected)
    {
        var result = PronunciationTextNormalizer.PrepareExpectedText(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PrepareExpectedText_OnlyStripsOnePrefix_NotChained()
    {
        var result = PronunciationTextNormalizer.PrepareExpectedText("a to go");

        Assert.Equal("to go", result);
    }

    [Fact]
    public void PrepareExpectedText_PrefixLikeWordIsNotStripped()
    {
        var result = PronunciationTextNormalizer.PrepareExpectedText("another word");

        Assert.Equal("another word", result);
    }

    [Fact]
    public void PrepareExpectedText_StandaloneArticleWord_IsNotStripped()
    {
        // "to" with no trailing content: after Trim() there's no "to " (with trailing space) to
        // match against, so it's returned as-is rather than becoming an empty string.
        var result = PronunciationTextNormalizer.PrepareExpectedText("to ");

        Assert.Equal("to", result);
    }

    [Fact]
    public void PrepareExpectedText_RemovesBracketedAnnotationBeforeStrippingPrefix()
    {
        var result = PronunciationTextNormalizer.PrepareExpectedText("to [formal] go");

        Assert.Equal("go", result);
    }
}
