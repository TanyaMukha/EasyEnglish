using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Interface for the base repository that provides common CRUD operations for an entity, backed by
/// a per-operation <see cref="Microsoft.EntityFrameworkCore.DbContext"/> created from an
/// <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}"/>.
/// </summary>
/// <typeparam name="T">The type of the entity. Must derive from <see cref="AbstractEntity"/>.</typeparam>
public interface IBaseRepository<T>
    where T : AbstractEntity
{
    /// <summary>
    /// Retrieves all entities (no filtering, sorting, or paging), read-only (no change tracking).
    /// </summary>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All entities matching the (optional) per-user scope.</returns>
    Task<IEnumerable<T>> GetAsync(string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities by dynamic query parameters (filters, sorting, pagination), translated by
    /// <c>MukhaLab.SelectQueryParameters</c> into a single SQL query. Read-only (no change tracking).
    /// </summary>
    /// <param name="parameters">Filtering, sorting, and paging instructions.</param>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entities matching <paramref name="parameters"/> and the (optional) per-user scope.</returns>
    Task<IEnumerable<T>> GetAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the total row count and page count for <paramref name="parameters"/>'s filters, with
    /// paging ignored (so the count reflects the whole filtered set, not a single page).
    /// </summary>
    /// <param name="parameters">Filtering instructions (sorting and paging are not applied to the count).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Finds an entity by its <c>int</c> primary key. Read-only (no change tracking). Applies per-user filtering when active.</summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching entity, or <c>null</c> if none exists or it isn't owned by the current user.</returns>
    Task<T?> FindAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an entity by primary key value(s), supporting composite keys. Does not support
    /// <paramref name="keyValues"/> together with eager-loaded includes. The single-<c>int</c>-key
    /// case applies per-user filtering when active; composite-key lookups do not.
    /// </summary>
    /// <param name="keyValues">The primary key value(s), in key-definition order.</param>
    /// <returns>The matching entity, or <c>null</c> if none exists or no key values were supplied.</returns>
    Task<T?> FindAsync(params object[] keyValues);

    /// <summary>Finds multiple entities by their <c>int</c> primary keys. Read-only (no change tracking). Applies per-user filtering when active.</summary>
    /// <param name="ids">Primary key values to look up.</param>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entities that were found and owned by the current user; missing/unowned ids are silently omitted.</returns>
    Task<List<T>> FindManyAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="FindManyAsync(IEnumerable{int}, string[], CancellationToken)"/>
    Task<List<T>> FindManyAsync(params int[] ids);

    /// <summary>Counts all entities in the (optionally per-user-scoped) table. Does not apply any <see cref="QueryParameters"/> filters.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Adds multiple new entities in a single <c>SaveChanges</c> call.</summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity. Marks every property as modified, so the full entity state is
    /// written. When per-user filtering is active, ownership is verified before the update is applied.
    /// </summary>
    /// <param name="entity">The entity to update (with its primary key set).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity.</returns>
    /// <exception cref="EntityNotFoundException">The entity does not exist, isn't owned by the current user, or was concurrently modified/deleted.</exception>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates multiple existing entities in a single <c>SaveChanges</c> call. When per-user filtering is active, ownership of every entity is verified first.</summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">One or more entities do not exist, aren't owned by the current user, or were concurrently modified/deleted.</exception>
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its unique identifier. When per-user filtering is active, ownership is
    /// verified before the delete is applied.
    /// </summary>
    /// <param name="keyValues">The primary key value(s) of the entity to delete.</param>
    /// <exception cref="EntityNotFoundException">The entity does not exist, isn't owned by the current user, or was concurrently modified/deleted.</exception>
    Task<bool> RemoveAsync(params object[] keyValues);

    /// <summary>
    /// Deletes entities by int primary keys in a single batch (one SELECT + one SaveChanges).
    /// All-or-nothing: if any id in <paramref name="ids"/> doesn't match an existing, currently-owned
    /// row, no rows are deleted.
    /// </summary>
    /// <param name="ids">Primary key values of the entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">One or more ids do not exist, aren't owned by the current user, or were concurrently modified/deleted.</exception>
    Task<bool> RemoveRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>Deletes entities by composite (or non-<c>int</c>) key value sets, one lookup per key set. When per-user filtering is active, ownership of every found entity is verified before any are removed.</summary>
    /// <param name="keyValuesList">The primary key value set of each entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">One or more entities do not exist, aren't owned by the current user, or were concurrently modified/deleted.</exception>
    Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes already-loaded <paramref name="entities"/> in a single <c>SaveChanges</c> call. When
    /// per-user filtering is active, ownership of every entity is verified before any are removed.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">One or more entities aren't owned by the current user, or were concurrently modified/deleted.</exception>
    Task<bool> RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
