using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface ITestCardRepository : IBaseRepository<TestCardEntity>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    /// <summary>Selects test cards from a course/unit for learning, according to the given options.</summary>
    Task<List<TestCardEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
}
