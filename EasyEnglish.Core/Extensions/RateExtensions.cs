using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;

namespace EasyEnglish.Core.Extensions;

public static class RateExtensions
{
    // Границы на шкале [0; 5]. Тюнингуются здесь.
    public const float EasyMax = 5f / 3f;   // ~1.67 — ниже этого «легко»
    public const float HardMin = 10f / 3f;  // ~3.33 — от этого и выше «тяжело»

    public static DifficultyLevel ToDifficulty(this float rate) => rate switch
    {
        < EasyMax => DifficultyLevel.Easy,
        < HardMin => DifficultyLevel.Medium,
        _ => DifficultyLevel.Hard
    };

    // Удобно, т.к. WordModel реализует IRateInfo
    public static DifficultyLevel ToDifficulty(this IRateInfo rateInfo)
        => rateInfo.Rate.ToDifficulty();
}
