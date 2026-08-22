using EasyPeasy.Core.Models;
using EasyPeasy.Core.Options;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Services;

/// <summary>Service for <see cref="StudyCardModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface IStudyCardService : IBaseService<StudyCardModel>
{
    /// <summary>
    /// Finds the previous/next study card id relative to <paramref name="currentCardId"/> within its
    /// unit, plus the card's 1-based position and the unit's total card count.
    /// </summary>
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    /// <summary>Applies review results (rate/date/count) to several study cards at once.</summary>
    Task<IEnumerable<StudyCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);

    /// <summary>Selects study cards from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<StudyCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of study cards reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}