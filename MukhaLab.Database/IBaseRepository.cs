using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Interface for the base service that provides common CRUD operations for an entity.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IBaseRepository<T>
    where T : class
{
    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAsync(QueryParameters parameters, bool disabledIncludes = false);

    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, bool disabledIncludes = false);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    Task<T?> FindAsync(params object[] keyValues);

    Task<List<T>> FindManyAsync(params int[] ids);

    Task<List<T>> FindManyAsync(IEnumerable<int> ids);

    Task<int> CountAsync();

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task<T> AddAsync(T entity);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    Task<T> UpdateAsync(T entity);

    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    Task<bool> RemoveAsync(params object[] keyValues);

    Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList);
}
