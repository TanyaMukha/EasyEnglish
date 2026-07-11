using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Extensions;
using EasyEnglish.Core.Interfaces.Fields;

namespace EasyEnglish.Core.Tests;

public class RateExtensionsTests
{
    [Theory]
    [InlineData(0f, DifficultyLevel.Easy)]
    [InlineData(1f, DifficultyLevel.Easy)]
    [InlineData(3f, DifficultyLevel.Medium)]
    [InlineData(5f, DifficultyLevel.Hard)]
    public void ToDifficulty_MidRangeValues_ReturnsExpectedBucket(float rate, DifficultyLevel expected)
    {
        Assert.Equal(expected, rate.ToDifficulty());
    }

    [Fact]
    public void ToDifficulty_JustBelowEasyMax_IsEasy()
    {
        var justBelow = RateExtensions.EasyMax - 0.0001f;

        Assert.Equal(DifficultyLevel.Easy, justBelow.ToDifficulty());
    }

    [Fact]
    public void ToDifficulty_ExactlyEasyMax_IsMedium()
    {
        // The comparison is `< EasyMax`, so the boundary value itself belongs to the next bucket up.
        Assert.Equal(DifficultyLevel.Medium, RateExtensions.EasyMax.ToDifficulty());
    }

    [Fact]
    public void ToDifficulty_JustBelowHardMin_IsMedium()
    {
        var justBelow = RateExtensions.HardMin - 0.0001f;

        Assert.Equal(DifficultyLevel.Medium, justBelow.ToDifficulty());
    }

    [Fact]
    public void ToDifficulty_ExactlyHardMin_IsHard()
    {
        Assert.Equal(DifficultyLevel.Hard, RateExtensions.HardMin.ToDifficulty());
    }

    private sealed class FakeRateInfo : IRateInfo
    {
        public float Rate { get; set; }
    }

    [Fact]
    public void ToDifficulty_IRateInfoOverload_DelegatesToFloatOverload()
    {
        IRateInfo rateInfo = new FakeRateInfo { Rate = 4.5f };

        Assert.Equal(DifficultyLevel.Hard, rateInfo.ToDifficulty());
    }
}
