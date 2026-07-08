using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IIrregularFormService : IBaseService<IrregularFormModel>
{
    Task<IEnumerable<IrregularFormModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> forms);

    /// <summary>Selects irregular forms from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<IrregularFormModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of irregular forms reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
