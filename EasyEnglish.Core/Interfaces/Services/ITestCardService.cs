using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface ITestCardService : IBaseService<TestCardModel>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    Task<IEnumerable<TestCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);

    /// <summary>Selects test cards from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<TestCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
}
