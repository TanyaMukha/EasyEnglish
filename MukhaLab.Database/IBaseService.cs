using MukhaLab.Database;
using MukhaLab.SelectQueryParameters.Models;

namespace EasyEnglish.Core.Interfaces.Services;

/// <summary>
/// Базовий інтерфейс для всіх сервісів з CRUD операціями
/// </summary>
/// <typeparam name="TModel">Тип моделі</typeparam>
public interface IBaseService<TModel>
    where TModel : class
{
    Task<IEnumerable<TModel>> GetAllAsync(QueryParameters? parameters = null, string[]? includes = null);
    Task<TModel?> GetByIdAsync(int id, string[]? includes = null);
    Task<List<TModel>> GetByIdsAsync(params int[] ids);
    Task<List<TModel>> GetByIdsAsync(IEnumerable<int> ids, string[]? includes = null);
    Task<int> CountAsync();
    Task<TModel> CreateAsync(TModel model);
    Task<TModel> UpdateAsync(int id, TModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<TModel>> CreateRangeAsync(IEnumerable<TModel> requests);
    Task<IEnumerable<TModel>> UpdateRangeAsync(IEnumerable<(int Id, TModel Model)> requests);
    Task<bool> DeleteRangeAsync(IEnumerable<int> ids);
    Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters);
}