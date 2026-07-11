using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

/// <summary>Service for <see cref="WordModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface IWordService : IBaseService<WordModel>
{
    /// <summary>Words that haven't been reviewed for the longest time, across any course/unit.</summary>
    Task<IEnumerable<WordModel>> GetAnyNextWordsAsync(int count);

    /// <summary>The most difficult words (by rating), across any course/unit.</summary>
    Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count);

    /// <summary>All words in a unit.</summary>
    Task<IEnumerable<WordModel>> GetByUnitAsync(int unitId, string[]? includes = null);

    /// <summary>Selects words from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<WordModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Applies a single review result (rate/date/count) to one word.</summary>
    Task<WordModel> UpdateWordRateAsync(UpdateWordRateRequest word);

    /// <summary>Applies review results (rate/date/count) to several words at once.</summary>
    Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words);

    /// <summary>
    /// Finds the previous/next word id relative to <paramref name="currentWordId"/> within its unit,
    /// plus the word's 1-based position and the unit's total word count — for prev/next navigation UI.
    /// </summary>
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId);

    /// <summary>Number of words reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}

/// <summary>
/// A single review result to apply to a learnable item (word, irregular form, study/test card) — the
/// shared request shape for every <c>Update*RateAsync</c>/<c>UpdateRateRangeAsync</c> method.
/// </summary>
public class UpdateWordRateRequest
{
    public int Id { get; init; }
    public float Rate { get; init; }
    public DateTime? LastReviewDate { get; init; }
    public int ReviewCount { get; init; }
}