using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Interface for the base service that provides common CRUD operations for an entity.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IBaseService<TModel>
    where TModel : class
{
    Task<IEnumerable<TModel>> GetAllAsync(string[]? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Вибірка за динамічними параметрами (фільтри, сортування, пагінація).
    /// Зарезервовано для майбутніх динамічних фільтрів; основні запити — LINQ у репозиторіях.
    /// </summary>
    Task<IEnumerable<TModel>> GetAllAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default);

    Task<TModel?> GetByIdAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default);
    Task<List<TModel>> GetByIdsAsync(params int[] ids);
    Task<List<TModel>> GetByIdsAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);
    Task<TModel> UpdateAsync(int id, TModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TModel>> CreateRangeAsync(IEnumerable<TModel> requests, CancellationToken cancellationToken = default);
    Task<IEnumerable<TModel>> UpdateRangeAsync(IEnumerable<(int Id, TModel Model)> requests, CancellationToken cancellationToken = default);
    Task<bool> DeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
}
