using EasyEnglish.App.Services;

namespace EasyEnglish.App.Tests;

public class TextBracketsRemoverServiceTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("no brackets here", "no brackets here")]
    [InlineData("hello [world]", "hello")]
    [InlineData("hello [world] there", "hello there")]
    [InlineData("text[note].", "text.")]
    [InlineData("a[note]b", "ab")]
    [InlineData("word[a][b]", "word")]
    [InlineData("[only brackets]", "")]
    [InlineData("run (formal)[old-fashioned]", "run (formal)")]
    public void RemoveBracketsText_RemovesBracketedAnnotationsWithoutLeavingDoubleSpaces(
        string? input, string expected)
    {
        var result = TextBracketsRemoverService.RemoveBracketsText(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveBracketsText_MultipleAnnotations_RemovesEachOne()
    {
        var result = TextBracketsRemoverService.RemoveBracketsText("go [formal] there [rare] now");

        Assert.Equal("go there now", result);
    }
}
