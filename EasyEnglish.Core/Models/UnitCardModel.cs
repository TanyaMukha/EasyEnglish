namespace EasyEnglish.Core.Models;

/// <summary>
/// Lightweight summary of a unit for a course overview list — avoids loading each unit's full word
/// list just to show progress. Counts are bucketed by <see cref="EasyEnglish.Core.Enums.DifficultyLevel"/>
/// (see <see cref="EasyEnglish.Core.Extensions.RateExtensions.ToDifficulty(float)"/>).
/// </summary>
public class UnitCardModel
{
    public int Id { get; set; }
    public Guid RecordGuid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int EasyCount { get; set; }
    public int MediumCount { get; set; }
    public int HardCount { get; set; }
    public int TotalCount { get; set; }
}
