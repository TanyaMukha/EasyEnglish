using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

/// <summary>Repository for <see cref="TestCardEntity"/>, beyond the generic CRUD in <see cref="IBaseRepository{T}"/>.</summary>
public interface ITestCardRepository : IBaseRepository<TestCardEntity>
{
    /// <summary>
    /// Finds the previous/next test card id relative to <paramref name="currentCardId"/> within its
    /// unit, plus the card's 1-based position and the unit's total card count.
    /// </summary>
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    /// <summary>Selects test cards from a course/unit for learning, according to the given options.</summary>
    Task<List<TestCardEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of test cards reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
