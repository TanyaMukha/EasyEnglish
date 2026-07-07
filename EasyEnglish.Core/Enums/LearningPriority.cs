namespace EasyEnglish.Core.Enums;

/// <summary>
/// Priority used to pick which items (words, irregular forms, cards) to select for learning.
/// </summary>
public enum LearningPriority
{
    /// <summary>
    /// Random items.
    /// </summary>
    Random = 0,

    /// <summary>
    /// Difficult items (with the most mistakes).
    /// </summary>
    Difficult = 1,

    /// <summary>
    /// New items (not yet reviewed).
    /// </summary>
    New = 2,

    /// <summary>
    /// Items due for review.
    /// </summary>
    Review = 3,

    /// <summary>
    /// Oldest items (not reviewed for the longest time).
    /// </summary>
    Old = 4
}
