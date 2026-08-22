using EasyPeasy.App.Services;
using static EasyPeasy.App.Services.ExampleMarkdownService;

namespace EasyPeasy.App.Tests;

public class ExampleMarkdownServiceTests
{
    [Theory]
    [InlineData("***bold italic***", "<strong><em>bold italic</em></strong>")]
    [InlineData("**bold**", "<strong>bold</strong>")]
    [InlineData("__italic__", "<em>italic</em>")]
    [InlineData("`code`", "<code>code</code>")]
    [InlineData("[link](https://example.com)", "<a href=\"https://example.com\">link</a>")]
    [InlineData("line one\nline two", "line one<br />line two")]
    public void RenderMarkdown_RendersEachMarkerToHtml(string input, string expected)
    {
        var result = RenderMarkdown(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RenderMarkdown_NullOrWhitespace_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, RenderMarkdown(null!));
        Assert.Equal(string.Empty, RenderMarkdown("   "));
    }

    [Fact]
    public void RenderMarkdown_HiddenMarkerSkipsThatPatternOnly()
    {
        var result = RenderMarkdown("**hidden** and __shown__", HiddenTextMarker.Bold);

        Assert.Equal("**hidden** and <em>shown</em>", result);
    }

    [Fact]
    public void ParseHiddenText_FindsFirstMatch()
    {
        var (before, hidden, after) = ParseHiddenText("The **cat** sat", HiddenTextMarker.Bold);

        Assert.Equal("The ", before);
        Assert.Equal("cat", hidden);
        Assert.Equal(" sat", after);
    }

    [Fact]
    public void ParseHiddenText_NoMatch_ReturnsWholeSentenceAsBefore()
    {
        var (before, hidden, after) = ParseHiddenText("no markers here", HiddenTextMarker.Bold);

        Assert.Equal("no markers here", before);
        Assert.Equal("", hidden);
        Assert.Equal("", after);
    }

    [Fact]
    public void ParseHiddenText_MarkerNone_ReturnsWholeSentenceAsBefore()
    {
        var (before, hidden, after) = ParseHiddenText("**cat**", HiddenTextMarker.None);

        Assert.Equal("**cat**", before);
        Assert.Equal("", hidden);
    }

    [Theory]
    [InlineData("The **cat** sat", true)]
    [InlineData("no markers here", false)]
    public void HasHiddenText_DetectsPresence(string sentence, bool expected)
    {
        Assert.Equal(expected, HasHiddenText(sentence, HiddenTextMarker.Bold));
    }

    [Fact]
    public void RenderMarkdownWithHidden_ShowHidden_UsesRevealedClass()
    {
        var result = RenderMarkdownWithHidden("The **cat** sat", showHidden: true);

        Assert.Equal("The <span class='hidden-text revealed'>cat</span> sat", result);
    }

    [Fact]
    public void RenderMarkdownWithHidden_HideHidden_UsesBlurredClass()
    {
        var result = RenderMarkdownWithHidden("The **cat** sat", showHidden: false);

        Assert.Equal("The <span class='hidden-text blurred'>cat</span> sat", result);
    }

    [Fact]
    public void StripMarkdown_RemovesAllMarkersButKeepsText()
    {
        var result = StripMarkdown("***a*** **b** __c__ `d`");

        Assert.Equal("a b c d", result);
    }

    [Fact]
    public void GetHiddenTextOnly_ReturnsJustTheHiddenSpan()
    {
        var result = GetHiddenTextOnly("The **cat** sat", HiddenTextMarker.Bold);

        Assert.Equal("cat", result);
    }

    [Fact]
    public void GetHiddenTextOnly_NoMatch_ReturnsEmpty()
    {
        var result = GetHiddenTextOnly("no markers", HiddenTextMarker.Bold);

        Assert.Equal("", result);
    }

    [Theory]
    [InlineData("cat", "cat", true)]
    [InlineData(" Cat ", "cat", true)]
    [InlineData("CAT", "cat", true)]
    [InlineData("dog", "cat", false)]
    public void CheckAnswer_IgnoresCaseAndSurroundingWhitespace(string userAnswer, string correctAnswer, bool expected)
    {
        Assert.Equal(expected, CheckAnswer(userAnswer, correctAnswer));
    }

    [Fact]
    public void ParseSegments_NoMarkers_ReturnsSingleVisibleSegment()
    {
        var result = ParseSegments("plain text", HiddenTextMarker.Bold);

        var segment = Assert.Single(result);
        Assert.False(segment.IsHidden);
        Assert.Equal("plain text", segment.Text);
    }

    [Fact]
    public void ParseSegments_MultipleOccurrences_AlternatesVisibleAndHidden()
    {
        var result = ParseSegments("The **cat** sat on the **mat**", HiddenTextMarker.Bold);

        Assert.Equal(
        [
            (false, "The "),
            (true, "cat"),
            (false, " sat on the "),
            (true, "mat"),
        ], result);
    }

    [Fact]
    public void ParseSegments_StartsWithHiddenSpan_NoLeadingEmptySegment()
    {
        var result = ParseSegments("**cat** sat", HiddenTextMarker.Bold);

        Assert.Equal(
        [
            (true, "cat"),
            (false, " sat"),
        ], result);
    }

    [Fact]
    public void ParseSegments_EmptyString_ReturnsEmptyList()
    {
        var result = ParseSegments("", HiddenTextMarker.Bold);

        Assert.Empty(result);
    }
}
