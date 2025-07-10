using MukhaLib.SelectQueryParameters.Models;

namespace MukhaLib.Database;

/// <summary>
/// Interface that defines common CRUD operations for models used in controllers.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface ICrud<TModel>
    where TModel : AbstractModel
{
    /// <summary>
    /// Retrieves all models.
    /// </summary>
    /// <returns>An <see cref="Task{T}"/> containing a collection of all models.</returns>
    Task<IEnumerable<TModel>> GetAll(string userId, string? disablePagination, GetRequestParameters parameters);

    /// <summary>
    /// Retrieves a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model.</param>
    /// <returns>An <see cref="Task{T}"/> containing the model if found.</returns>
    Task<TModel> GetById(string userId, int id);

    /// <summary>
    /// Adds a new model.
    /// </summary>
    /// <param name="model">The model to add.</param>
    /// <returns>An <see cref="Task{T}"/> containing the added model.</returns>
    Task<TModel> Add(string userId, TModel model);

    /// <summary>
    /// Updates an existing model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model to update.</param>
    /// <param name="model">The model with updated values.</param>
    /// <returns>An <see cref="Task{T}"/> containing the updated model.</returns>
    Task<TModel> Update(string userId, int id, TModel model);

    /// <summary>
    /// Deletes a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model to delete.</param>
    /// <returns>An <see cref="Task"/> indicating the result of the operation.</returns>
    Task Delete(string userId, int id);
}
