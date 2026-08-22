using EasyPeasy.App.Services.Speech;

namespace EasyPeasy.App.Tests;

public class TextChunkParserTests
{
    private const SpeechLanguage Primary = SpeechLanguage.EnglishBritish;
    private const SpeechLanguage Inclusion = SpeechLanguage.Ukrainian;

    [Fact]
    public void Parse_NoMarkers_ReturnsWholeTextAsSinglePrimaryChunk()
    {
        var result = TextChunkParser.Parse("hello world", Primary, Inclusion);

        var chunk = Assert.Single(result);
        Assert.Equal(new TextChunk("hello world", Primary), chunk);
    }

    [Fact]
    public void Parse_MarkerInTheMiddle_SplitsIntoThreeChunks()
    {
        var result = TextChunkParser.Parse("The **cat** sat", Primary, Inclusion);

        Assert.Equal(
        [
            new TextChunk("The", Primary),
            new TextChunk("cat", Inclusion),
            new TextChunk("sat", Primary),
        ], result);
    }

    [Fact]
    public void Parse_TextStartsWithMarker_NoLeadingEmptyChunk()
    {
        var result = TextChunkParser.Parse("**cat** sat", Primary, Inclusion);

        Assert.Equal(
        [
            new TextChunk("cat", Inclusion),
            new TextChunk("sat", Primary),
        ], result);
    }

    [Fact]
    public void Parse_TextEndsWithMarker_NoTrailingEmptyChunk()
    {
        var result = TextChunkParser.Parse("The **cat**", Primary, Inclusion);

        Assert.Equal(
        [
            new TextChunk("The", Primary),
            new TextChunk("cat", Inclusion),
        ], result);
    }

    [Fact]
    public void Parse_MultipleMarkers_AlternatesLanguages()
    {
        var result = TextChunkParser.Parse("**a** and **b**", Primary, Inclusion);

        Assert.Equal(
        [
            new TextChunk("a", Inclusion),
            new TextChunk("and", Primary),
            new TextChunk("b", Inclusion),
        ], result);
    }

    [Fact]
    public void Parse_WhitespaceAroundMarkerContent_IsTrimmed()
    {
        var result = TextChunkParser.Parse("The **  cat  ** sat", Primary, Inclusion);

        Assert.Equal(
        [
            new TextChunk("The", Primary),
            new TextChunk("cat", Inclusion),
            new TextChunk("sat", Primary),
        ], result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespaceOnly_ReturnsEmptyList(string text)
    {
        var result = TextChunkParser.Parse(text, Primary, Inclusion);

        Assert.Empty(result);
    }
}
