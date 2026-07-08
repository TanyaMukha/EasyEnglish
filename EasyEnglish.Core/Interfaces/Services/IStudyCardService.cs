using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IStudyCardService : IBaseService<StudyCardModel>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    Task<IEnumerable<StudyCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);

    /// <summary>Selects study cards from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<StudyCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of study cards reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}