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
    Task<IEnumerable<T>> GetAsync(string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities by dynamic query parameters (filters, sorting, pagination).
    /// Зарезервовано для майбутніх динамічних фільтрів; основні запити — LINQ у репозиторіях.
    /// </summary>
    Task<IEnumerable<T>> GetAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default);

    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(params object[] keyValues);
    Task<List<T>> FindManyAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default);
    Task<List<T>> FindManyAsync(params int[] ids);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    Task<bool> RemoveAsync(params object[] keyValues);

    /// <summary>
    /// Deletes entities by int primary keys in a single batch (one SELECT + one SaveChanges).
    /// </summary>
    Task<bool> RemoveRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList, CancellationToken cancellationToken = default);
}
