using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IIrregularFormRepository : IBaseRepository<IrregularFormEntity>
{
    /// <summary>Selects irregular forms from a course/unit for learning, according to the given options.</summary>
    Task<List<IrregularFormEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
}
