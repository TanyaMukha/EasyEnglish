using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Options;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Repositories;

/// <summary>Repository for <see cref="IrregularFormEntity"/>, beyond the generic CRUD in <see cref="IBaseRepository{T}"/>.</summary>
public interface IIrregularFormRepository : IBaseRepository<IrregularFormEntity>
{
    /// <summary>Selects irregular forms from a course/unit for learning, according to the given options.</summary>
    Task<List<IrregularFormEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);

    /// <summary>Number of irregular forms reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}
