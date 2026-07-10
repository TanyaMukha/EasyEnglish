using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IUnitService : IBaseWithGuidService<UnitModel>
{
    Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId);

    Task<IEnumerable<UnitModel>> GetByCourseAsync(int courseId);

    Task<IEnumerable<WordModel>> GetWordsAsync(int unitId, string[]? includes = null);

    /// <summary>All examples belonging to a unit's words, as a flat list.</summary>
    Task<IEnumerable<ExampleModel>> GetExamplesAsync(int unitId);

    /// <summary>
    /// Updates a unit together with its children, matching each child against the unit's existing
    /// ones by RecordGuid instead of blindly cascading by Id (which would duplicate everything whose
    /// Id was reset to 0 during import mapping). See implementation remarks for details.
    /// </summary>
    Task<UnitModel> ReconcileAndUpdateAsync(UnitModel incoming, bool deleteMissing, CancellationToken cancellationToken = default);
}