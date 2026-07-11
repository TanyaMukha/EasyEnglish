using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;

namespace EasyEnglish.Core.Extensions;

/// <summary>Converts a numeric <c>Rate</c> (scale <c>[0, 5]</c>) into a <see cref="DifficultyLevel"/> bucket.</summary>
public static class RateExtensions
{
    // Thresholds split the [0, 5] scale into three equal thirds. Tune here — every consumer
    // (UnitCardModel counts, UI badges, etc.) goes through ToDifficulty, not these constants directly.

    /// <summary>Upper bound (exclusive) of <see cref="DifficultyLevel.Easy"/>: <c>5/3 ≈ 1.67</c>.</summary>
    public const float EasyMax = 5f / 3f;

    /// <summary>Lower bound (inclusive) of <see cref="DifficultyLevel.Hard"/>: <c>10/3 ≈ 3.33</c>.</summary>
    public const float HardMin = 10f / 3f;

    /// <summary>
    /// Maps a rate to a difficulty bucket: <c>&lt; EasyMax</c> is <see cref="DifficultyLevel.Easy"/>,
    /// <c>&lt; HardMin</c> is <see cref="DifficultyLevel.Medium"/>, otherwise <see cref="DifficultyLevel.Hard"/>.
    /// </summary>
    public static DifficultyLevel ToDifficulty(this float rate) => rate switch
    {
        < EasyMax => DifficultyLevel.Easy,
        < HardMin => DifficultyLevel.Medium,
        _ => DifficultyLevel.Hard
    };

    /// <summary>Convenience overload for any <see cref="IRateInfo"/> (entities/models all implement it).</summary>
    public static DifficultyLevel ToDifficulty(this IRateInfo rateInfo)
        => rateInfo.Rate.ToDifficulty();
}
