using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IStudyCardRepository : IBaseRepository<StudyCardEntity>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    /// <summary>Selects study cards from a course/unit for learning, according to the given options.</summary>
    Task<List<StudyCardEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of study cards reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
