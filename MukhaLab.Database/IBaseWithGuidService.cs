namespace MukhaLab.Database;

/// <summary>
/// Extends <see cref="IBaseService{TModel}"/> with lookups keyed by <see cref="IGuidRecord.RecordGuid"/>.
/// </summary>
/// <typeparam name="TModel">The type of the model. Must implement <see cref="IGuidRecord"/>.</typeparam>
public interface IBaseWithGuidService<TModel> : IBaseService<TModel>
    where TModel : class, IGuidRecord
{
    /// <summary>Retrieves a model by its <see cref="IGuidRecord.RecordGuid"/>.</summary>
    /// <param name="guid">The record GUID to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapped model, or <c>null</c> if no matching entity exists.</returns>
    Task<TModel?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Filters <paramref name="guids"/> down to the ones that already exist in the table.</summary>
    /// <param name="guids">Candidate GUIDs to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subset of <paramref name="guids"/> that already exist.</returns>
    Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default);
}
