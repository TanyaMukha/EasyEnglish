using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

/// <summary>Service for <see cref="UnitModel"/>, beyond the generic CRUD in <see cref="IBaseWithGuidService{TModel}"/>.</summary>
public interface IUnitService : IBaseWithGuidService<UnitModel>
{
    /// <summary>Units of a course as lightweight <see cref="UnitCardModel"/> summaries.</summary>
    Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId);

    /// <summary>All units belonging to a course.</summary>
    Task<IEnumerable<UnitModel>> GetByCourseAsync(int courseId);

    /// <summary>All words in a unit.</summary>
    Task<IEnumerable<WordModel>> GetWordsAsync(int unitId, string[]? includes = null);

    /// <summary>All examples belonging to a unit's words, as a flat list.</summary>
    Task<IEnumerable<ExampleModel>> GetExamplesAsync(int unitId);

    /// <summary>
    /// Updates a unit together with its children, matching each child against the unit's existing
    /// ones by RecordGuid instead of blindly cascading by Id (which would duplicate everything whose
    /// Id was reset to 0 during import mapping). <paramref name="options"/> declares what the
    /// incoming graph is authoritative for, so a partial payload doesn't destroy data it doesn't
    /// carry. See implementation remarks for details.
    /// </summary>
    Task<UnitModel> ReconcileAndUpdateAsync(UnitModel incoming, UnitMergeOptions options, CancellationToken cancellationToken = default);
}