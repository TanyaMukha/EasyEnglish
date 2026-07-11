namespace EasyEnglish.Core.Enums;

/// <summary>
/// Difficulty bucket derived from an item's <see cref="Interfaces.Fields.IRateInfo.Rate"/> via
/// <see cref="Extensions.RateExtensions.ToDifficulty(float)"/>. Not stored directly — always computed.
/// </summary>
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}
