namespace MukhaLab.Database;

/// <summary>
/// Extends <see cref="IBaseRepository{T}"/> with lookups keyed by <see cref="IGuidRecord.RecordGuid"/>
/// instead of the <c>int</c> primary key.
/// </summary>
/// <typeparam name="T">The type of the entity. Must derive from <see cref="AbstractEntity"/> and implement <see cref="IGuidRecord"/>.</typeparam>
public interface IBaseWithGuidRepository<T> : IBaseRepository<T>
    where T : AbstractEntity, IGuidRecord
{
    /// <summary>Finds an entity by its <see cref="IGuidRecord.RecordGuid"/>. Read-only (no change tracking).</summary>
    /// <param name="guid">The record GUID to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching entity, or <c>null</c> if none exists.</returns>
    Task<T?> FindAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Filters <paramref name="guids"/> down to the ones that already exist in the table.</summary>
    /// <param name="guids">Candidate GUIDs to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subset of <paramref name="guids"/> that already exist.</returns>
    Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default);
}
