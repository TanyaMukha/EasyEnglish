using EasyPeasy.Core.Models;
using EasyPeasy.Core.Options;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Services;

/// <summary>Service for <see cref="IrregularFormModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface IIrregularFormService : IBaseService<IrregularFormModel>
{
    /// <summary>Applies review results (rate/date/count) to several irregular forms at once.</summary>
    Task<IEnumerable<IrregularFormModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> forms);

    /// <summary>Selects irregular forms from a course/unit for learning, according to the given options.</summary>
    Task<IEnumerable<IrregularFormModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of irregular forms reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
