using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

/// <summary>Service for <see cref="TestCardModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface ITestCardService : IBaseService<TestCardModel>
{
    /// <summary>
    /// Finds the previous/next test card id relative to <paramref name="currentCardId"/> within its
    /// unit, plus the card's 1-based position and the unit's total card count.
    /// </summary>
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    /// <summary>Applies review results (rate/date/count) to several test cards at once.</summary>
    Task<IEnumerable<TestCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);

    /// <summary>Selects test cards from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<TestCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of test cards reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
