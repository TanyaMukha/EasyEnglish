using AutoMapper;
using EasyEnglish.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;
using MukhaLab.Database;

namespace EasyEnglish.Services;

public abstract class BaseService<TEntity, TModel> : IBaseService<TModel>
    where TEntity : AbstractEntity
    where TModel : class
{
    protected readonly IBaseRepository<TEntity> _repository;
    protected readonly IMapper _mapper;
    protected readonly ILogger<BaseService<TEntity, TModel>> _logger;

    protected BaseService(
        IBaseRepository<TEntity> repository,
        IMapper mapper,
        ILogger<BaseService<TEntity, TModel>> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<IEnumerable<TModel>> GetAllAsync(QueryParameters? parameters = null, string[]? includes = null)
    {
        try
        {
            _logger.LogDebug("Отримання всіх записів типу {EntityType}", typeof(TEntity).Name);

            parameters ??= new QueryParameters();
            var entities = await _repository.GetAsync(parameters, includes);
            return _mapper.Map<IEnumerable<TModel>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні всіх записів типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TModel?> GetByIdAsync(int id, string[]? includes = null)
    {
        try
        {
            _logger.LogDebug("Отримання запису з ID {Id} типу {EntityType}", id, typeof(TEntity).Name);

            var entity = await _repository.FindAsync(id, includes);
            if (entity == null)
            {
                _logger.LogWarning("Запис з ID {Id} не знайдено", id);
                return null;
            }

            var model = _mapper.Map<TModel>(entity);
            _logger.LogDebug("Успішно отримано запис з ID {Id}", id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні запису з ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Отримує записи за кількома ідентифікаторами.
    /// </summary>
    public virtual async Task<List<TModel>> GetByIdsAsync(params int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<TModel>();
        }

        return await GetByIdsAsync((IEnumerable<int>)ids);
    }

    /// <summary>
    /// Отримує записи за кількома ідентифікаторами.
    /// </summary>
    public virtual async Task<List<TModel>> GetByIdsAsync(IEnumerable<int> ids, string[]? includes = null)
    {
        try
        {
            if (ids == null || !ids.Any())
            {
                return new List<TModel>();
            }

            var idList = ids.ToList();
            _logger.LogDebug("Отримання {Count} записів типу {EntityType} за ідентифікаторами",
                idList.Count, typeof(TEntity).Name);

            var entities = await _repository.FindManyAsync(idList, includes);

            if (entities == null || entities.Count == 0)
            {
                _logger.LogWarning("Записи з вказаними ідентифікаторами не знайдено");
                return new List<TModel>();
            }

            var models = _mapper.Map<List<TModel>>(entities);

            _logger.LogDebug("Успішно отримано {Count} записів з {Requested} запитаних",
                models.Count, idList.Count);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні записів за ідентифікаторами");
            throw;
        }
    }

    public virtual async Task<int> CountAsync()
    {
        try
        {
            _logger.LogDebug("Підрахунок записів типу {EntityType}", typeof(TEntity).Name);
            int count = await _repository.CountAsync();
            _logger.LogDebug("Загальна кількість записів: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при підрахунку записів типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TModel> CreateAsync(TModel request)
    {
        try
        {
            _logger.LogDebug("Створення нового запису типу {EntityType}", typeof(TEntity).Name);

            var entity = _mapper.Map<TEntity>(request);

            await _repository.AddAsync(entity);

            var model = _mapper.Map<TModel>(entity);
            _logger.LogInformation("Створено новий запис з ID {Id}", entity.Id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні запису типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TModel> UpdateAsync(int id, TModel request)
    {
        try
        {
            _logger.LogDebug("Оновлення запису з ID {Id} типу {EntityType}", id, typeof(TEntity).Name);

            var existingEntity = await _repository.FindAsync(id);
            if (existingEntity == null)
            {
                throw new ArgumentException($"Запис з ID {id} не знайдено");
            }

            _mapper.Map(request, existingEntity);

            await _repository.UpdateAsync(existingEntity);

            var model = _mapper.Map<TModel>(existingEntity);
            _logger.LogInformation("Оновлено запис з ID {Id}", id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні запису з ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogDebug("Видалення запису з ID {Id} типу {EntityType}", id, typeof(TEntity).Name);

            var entity = await _repository.FindAsync(id);
            if (entity == null)
            {
                throw new ArgumentException($"Запис з ID {id} не знайдено");
            }

            bool res = await _repository.RemoveAsync(id);

            _logger.LogInformation("Видалено запис з ID {Id}", id);

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні запису з ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates multiple records asynchronously.
    /// </summary>
    public virtual async Task<IEnumerable<TModel>> CreateRangeAsync(IEnumerable<TModel> requests)
    {
        try
        {
            _logger.LogDebug("Створення {Count} записів типу {EntityType}", requests.Count(), typeof(TEntity).Name);

            var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
            await _repository.AddRangeAsync(entities);

            var models = _mapper.Map<IEnumerable<TModel>>(entities);
            _logger.LogInformation("Створено {Count} записів типу {EntityType}", models.Count(), typeof(TEntity).Name);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні декількох записів типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Updates multiple records asynchronously.
    /// </summary>
    public virtual async Task<IEnumerable<TModel>> UpdateRangeAsync(IEnumerable<(int Id, TModel Model)> requests)
    {
        try
        {
            _logger.LogDebug("Оновлення {Count} записів типу {EntityType}", requests.Count(), typeof(TEntity).Name);

            var requestsList = requests.ToList();
            var ids = requestsList.Select(r => r.Id).ToList();

            // ✅ Отримуємо всі entities за один раз
            var existingEntities = await _repository.FindManyAsync(ids);

            if (existingEntities.Count != requestsList.Count)
            {
                var foundIds = existingEntities.Select(e => e.Id).ToHashSet();
                var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();
                throw new ArgumentException($"Записи з ID {string.Join(", ", missingIds)} не знайдено");
            }

            // Створюємо словник для швидкого пошуку
            var entitiesDict = existingEntities.ToDictionary(e => e.Id);

            // ✅ Маппимо models на існуючі entities
            foreach (var (id, model) in requestsList)
            {
                if (entitiesDict.TryGetValue(id, out var existingEntity))
                {
                    _mapper.Map(model, existingEntity);
                }
            }

            // ✅ Оновлюємо всі entities
            await _repository.UpdateRangeAsync(existingEntities);

            var models = _mapper.Map<IEnumerable<TModel>>(existingEntities);
            _logger.LogInformation("Оновлено {Count} записів типу {EntityType}", models.Count(), typeof(TEntity).Name);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні декількох записів типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Deletes multiple records by their IDs asynchronously.
    /// </summary>
    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<int> ids)
    {
        try
        {
            _logger.LogDebug("Видалення {Count} записів типу {EntityType}", ids.Count(), typeof(TEntity).Name);

            var keyValuesList = ids.Select(id => new object[] { id }).ToList();
            bool result = await _repository.RemoveRangeAsync(keyValuesList);

            _logger.LogInformation("Видалено {Count} записів типу {EntityType}", ids.Count(), typeof(TEntity).Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні декількох записів типу {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters)
    {
        try
        {
            _logger.LogDebug("Отримання інформації про пагінацію для типу {EntityType}", typeof(TEntity).Name);

            var paginationInfo = await _repository.GetPaginationInfoAsync(parameters);

            _logger.LogDebug("Пагінація: загальна кількість {TotalCount}, сторінок {TotalPages}",
                paginationInfo.TotalCount, paginationInfo.TotalPages);

            return paginationInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні інформації про пагінацію");
            throw;
        }
    }

    [Obsolete("Use GetAllAsync instead")]
    public virtual IEnumerable<TModel> GetAll(QueryParameters? parameters = null)
    {
        return GetAllAsync(parameters).GetAwaiter().GetResult();
    }

    [Obsolete("Use GetByIdAsync instead")]
    public virtual TModel? GetById(int id)
    {
        return GetByIdAsync(id).GetAwaiter().GetResult();
    }

    [Obsolete("Use CreateAsync instead")]
    public virtual TModel Create(TModel request)
    {
        return CreateAsync(request).GetAwaiter().GetResult();
    }

    [Obsolete("Use UpdateAsync instead")]
    public virtual TModel Update(int id, TModel request)
    {
        var result = UpdateAsync(id, request).GetAwaiter().GetResult();
        return result ?? throw new InvalidOperationException("Update failed");
    }

    [Obsolete("Use DeleteAsync instead")]
    public virtual void Delete(int id)
    {
        DeleteAsync(id).GetAwaiter().GetResult();
    }
}