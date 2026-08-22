using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Options;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Repositories;

/// <summary>Repository for <see cref="WordEntity"/>, beyond the generic CRUD in <see cref="IBaseRepository{T}"/>.</summary>
public interface IWordRepository : IBaseRepository<WordEntity>
{
    /// <summary>
    /// Finds the previous/next word id relative to <paramref name="currentWordId"/> within its unit,
    /// plus the word's 1-based position and the unit's total word count — for prev/next navigation UI.
    /// </summary>
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId);

    /// <summary>Words that haven't been reviewed for the longest time.</summary>
    Task<List<WordEntity>> GetNextWordsAsync(int count);

    /// <summary>The most difficult words (by rating).</summary>
    Task<List<WordEntity>> GetHardWordsAsync(int count);

    /// <summary>All words in a unit.</summary>
    Task<List<WordEntity>> GetByUnitAsync(int unitId, string[]? includes = null);

    /// <summary>Selects words from a course/unit for learning, according to the given options.</summary>
    Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of words reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
