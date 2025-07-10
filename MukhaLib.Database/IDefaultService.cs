using MukhaLib.SelectQueryParameters.Models;

namespace MukhaLib.Database;

/// <summary>
/// Interface for the base service that provides common CRUD operations for an entity.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IDefaultService<T>
    where T : AbstractEntity
{
    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    IEnumerable<T> GetAll(GetRequestParameters parameters);

    PaginationInfo GetPaginationInfo(GetRequestParameters parameters);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    T? GetById(int id);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    void Update(T entity);

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(T entity);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    void DeleteById(int id);

    void ConfigureIncludes(string[] includes);

    void ConfigureUserIdField(string[] userIdPropertyPaths);
}
