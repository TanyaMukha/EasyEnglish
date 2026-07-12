using EasyEnglish.App.Services;

namespace EasyEnglish.App.Tests;

public class LanguageFlagHelperTests
{
    [Theory]
    [InlineData("en-us", "flags/us.svg")]
    [InlineData("uk-ua", "flags/ua.svg")]
    [InlineData("en-GB", "flags/gb.svg")]
    [InlineData("en_us", "flags/us.svg")]
    [InlineData("en--us", "flags/us.svg")]
    public void GetFlagIconPath_ValidCode_ReturnsRegionFlag(string code, string expected)
    {
        var result = LanguageFlagHelper.GetFlagIconPath(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("eng")]
    [InlineData("en-1a")]
    [InlineData("en-u1")]
    public void GetFlagIconPath_MissingOrUnrecognizableRegion_ReturnsNeutralFlag(string? code)
    {
        var result = LanguageFlagHelper.GetFlagIconPath(code);

        Assert.Equal("flags/xx.svg", result);
    }

    [Fact]
    public void GetFlagIconPath_BareTwoLetterCodeWithNoSeparator_IsTreatedAsARegion()
    {
        // Characterization of current behavior: a code with no '-'/'_' separator falls through to
        // using the whole (2-letter) code as if it were the region, since Split(...).LastOrDefault()
        // just returns the original string unchanged when there's nothing to split on.
        var result = LanguageFlagHelper.GetFlagIconPath("en");

        Assert.Equal("flags/en.svg", result);
    }
}
