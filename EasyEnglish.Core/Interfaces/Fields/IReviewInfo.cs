namespace EasyEnglish.Core.Interfaces.Fields;

/// <summary>Implemented by entities/models that track spaced-repetition review state.</summary>
public interface IReviewInfo
{
    /// <summary>When this item was last reviewed, or <c>null</c> if it has never been reviewed.</summary>
    DateTime? LastReviewDate { get; set; }

    /// <summary>How many times this item has been reviewed.</summary>
    int ReviewCount { get; set; }
}
