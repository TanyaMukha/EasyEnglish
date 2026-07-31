namespace EasyEnglish.Core.Models;

/// <summary>
/// Lightweight summary of a unit for a course overview list — avoids loading each unit's full
/// content just to show progress. Every count spans all four kinds of learnable content the unit
/// owns (words, irregular forms, study cards, test cards); the difficulty buckets follow
/// <see cref="EasyEnglish.Core.Enums.DifficultyLevel"/>
/// (see <see cref="EasyEnglish.Core.Extensions.RateExtensions.ToDifficulty(float)"/>).
/// </summary>
public class UnitCardModel
{
    public int Id { get; set; }
    public Guid RecordGuid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int WordCount { get; set; }
    public int IrregularFormCount { get; set; }
    public int StudyCardCount { get; set; }
    public int TestCardCount { get; set; }

    public int EasyCount { get; set; }
    public int MediumCount { get; set; }
    public int HardCount { get; set; }

    /// <summary>Sum of all four content counts — <see cref="EasyCount"/> + <see cref="MediumCount"/> + <see cref="HardCount"/> adds up to this.</summary>
    public int TotalCount { get; set; }
}
