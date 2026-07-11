using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Interface for the base service that provides common CRUD operations for a model, mapped from an
/// underlying entity accessed through an <see cref="IBaseRepository{T}"/>.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IBaseService<TModel>
    where TModel : class
{
    /// <summary>Retrieves all models (no filtering, sorting, or paging).</summary>
    /// <param name="includes">Navigation property paths to eager-load on the underlying entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<TModel>> GetAllAsync(string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves models by dynamic query parameters (filters, sorting, pagination).
    /// </summary>
    /// <param name="parameters">Filtering, sorting, and paging instructions.</param>
    /// <param name="includes">Navigation property paths to eager-load on the underlying entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<TModel>> GetAllAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a model by its <c>int</c> id.</summary>
    /// <param name="id">The entity id.</param>
    /// <param name="includes">Navigation property paths to eager-load on the underlying entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapped model, or <c>null</c> if no matching entity exists.</returns>
    Task<TModel?> GetByIdAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetByIdsAsync(IEnumerable{int}, string[], CancellationToken)"/>
    Task<List<TModel>> GetByIdsAsync(params int[] ids);

    /// <summary>Retrieves multiple models by their <c>int</c> ids.</summary>
    /// <param name="ids">Entity ids to look up.</param>
    /// <param name="includes">Navigation property paths to eager-load on the underlying entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The models that were found; missing ids are silently omitted.</returns>
    Task<List<TModel>> GetByIdsAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>Counts all entities backing this model. Does not apply any <see cref="QueryParameters"/> filters.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a new entity from <paramref name="model"/>.</summary>
    /// <param name="model">The model to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created model, mapped back from the persisted entity.</returns>
    Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);

    /// <summary>Updates the entity identified by <paramref name="id"/> with values from <paramref name="model"/>.</summary>
    /// <param name="id">The id of the entity to update.</param>
    /// <param name="model">The model containing the new values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated model.</returns>
    /// <exception cref="EntityNotFoundException">No entity with the given <paramref name="id"/> exists.</exception>
    Task<TModel> UpdateAsync(int id, TModel model, CancellationToken cancellationToken = default);

    /// <summary>Deletes the entity identified by <paramref name="id"/>.</summary>
    /// <param name="id">The id of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">No entity with the given <paramref name="id"/> exists.</exception>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates multiple new entities from <paramref name="requests"/>.</summary>
    /// <param name="requests">The models to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<TModel>> CreateRangeAsync(IEnumerable<TModel> requests, CancellationToken cancellationToken = default);

    /// <summary>Updates multiple entities identified by id in a single batch.</summary>
    /// <param name="requests">Id/model pairs describing which entity to update with which values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">One or more requested ids do not exist.</exception>
    Task<IEnumerable<TModel>> UpdateRangeAsync(IEnumerable<(int Id, TModel Model)> requests, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple entities by id in a single batch.</summary>
    /// <param name="ids">Ids of the entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> DeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the total row count and page count for <paramref name="parameters"/>'s filters, with
    /// paging ignored (so the count reflects the whole filtered set, not a single page).
    /// </summary>
    /// <param name="parameters">Filtering instructions (sorting and paging are not applied to the count).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
}
