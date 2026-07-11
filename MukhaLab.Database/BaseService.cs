using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Base implementation of a service that maps between an entity (<typeparamref name="TEntity"/>)
/// and its business-layer model (<typeparamref name="TModel"/>) via AutoMapper, delegating
/// persistence to an <see cref="IBaseRepository{T}"/>. Every method logs at <c>Debug</c>/<c>Information</c>
/// on success and at <c>Error</c> before rethrowing on failure.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must derive from <see cref="AbstractEntity"/>.</typeparam>
/// <typeparam name="TModel">The business-layer model type.</typeparam>
public abstract class BaseService<TEntity, TModel> : IBaseService<TModel>
    where TEntity : AbstractEntity
    where TModel : class
{
    /// <summary>Repository used to persist and query <typeparamref name="TEntity"/>.</summary>
    protected readonly IBaseRepository<TEntity> _repository;

    /// <summary>AutoMapper instance used to map between <typeparamref name="TEntity"/> and <typeparamref name="TModel"/>.</summary>
    protected readonly IMapper _mapper;

    /// <summary>Logger for this service.</summary>
    protected readonly ILogger<BaseService<TEntity, TModel>> _logger;

    /// <summary>Initializes the service.</summary>
    /// <param name="repository">Repository used to persist and query <typeparamref name="TEntity"/>.</param>
    /// <param name="mapper">AutoMapper instance used to map between <typeparamref name="TEntity"/> and <typeparamref name="TModel"/>.</param>
    /// <param name="logger">Logger for this service.</param>
    protected BaseService(
        IBaseRepository<TEntity> repository,
        IMapper mapper,
        ILogger<BaseService<TEntity, TModel>> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TModel>> GetAllAsync(string[]? includes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving all records of type {EntityType}", typeof(TEntity).Name);

            var entities = await _repository.GetAsync(includes, cancellationToken);
            return _mapper.Map<IEnumerable<TModel>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TModel>> GetAllAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving records of type {EntityType} by dynamic query parameters", typeof(TEntity).Name);

            var entities = await _repository.GetAsync(parameters, includes, cancellationToken);
            return _mapper.Map<IEnumerable<TModel>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TModel?> GetByIdAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving record with id {Id} of type {EntityType}", id, typeof(TEntity).Name);

            var entity = await _repository.FindAsync(id, includes, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Record with id {Id} was not found", id);
                return null;
            }

            var model = _mapper.Map<TModel>(entity);
            _logger.LogDebug("Successfully retrieved record with id {Id}", id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve record with id {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<List<TModel>> GetByIdsAsync(params int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<TModel>();
        }

        return await GetByIdsAsync((IEnumerable<int>)ids);
    }

    /// <inheritdoc/>
    public virtual async Task<List<TModel>> GetByIdsAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ids == null || !ids.Any())
            {
                return new List<TModel>();
            }

            var idList = ids.ToList();
            _logger.LogDebug("Retrieving {Count} records of type {EntityType} by id",
                idList.Count, typeof(TEntity).Name);

            var entities = await _repository.FindManyAsync(idList, includes, cancellationToken);

            if (entities == null || entities.Count == 0)
            {
                _logger.LogWarning("No records were found for the given ids");
                return new List<TModel>();
            }

            var models = _mapper.Map<List<TModel>>(entities);

            _logger.LogDebug("Successfully retrieved {Count} of {Requested} requested records",
                models.Count, idList.Count);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve records by id");
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting records of type {EntityType}", typeof(TEntity).Name);
            int count = await _repository.CountAsync(cancellationToken);
            _logger.LogDebug("Total record count: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TModel> CreateAsync(TModel request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating a new record of type {EntityType}", typeof(TEntity).Name);

            var entity = _mapper.Map<TEntity>(request);

            await _repository.AddAsync(entity, cancellationToken);

            var model = _mapper.Map<TModel>(entity);
            _logger.LogInformation("Created a new record with id {Id}", entity.Id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a record of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="EntityNotFoundException">No entity with the given <paramref name="id"/> exists.</exception>
    public virtual async Task<TModel> UpdateAsync(int id, TModel request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating record with id {Id} of type {EntityType}", id, typeof(TEntity).Name);

            var existingEntity = await _repository.FindAsync(id, cancellationToken: cancellationToken);
            if (existingEntity == null)
            {
                throw new EntityNotFoundException($"Record with id {id} was not found");
            }

            _mapper.Map(request, existingEntity);

            await _repository.UpdateAsync(existingEntity, cancellationToken);

            var model = _mapper.Map<TModel>(existingEntity);
            _logger.LogInformation("Updated record with id {Id}", id);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update record with id {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="EntityNotFoundException">No entity with the given <paramref name="id"/> exists.</exception>
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting record with id {Id} of type {EntityType}", id, typeof(TEntity).Name);

            var entity = await _repository.FindAsync(id, cancellationToken: cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException($"Record with id {id} was not found");
            }

            bool res = await _repository.RemoveAsync(id);

            _logger.LogInformation("Deleted record with id {Id}", id);

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete record with id {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TModel>> CreateRangeAsync(IEnumerable<TModel> requests, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating {Count} records of type {EntityType}", requests.Count(), typeof(TEntity).Name);

            var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
            await _repository.AddRangeAsync(entities, cancellationToken);

            var models = _mapper.Map<IEnumerable<TModel>>(entities);
            _logger.LogInformation("Created {Count} records of type {EntityType}", models.Count(), typeof(TEntity).Name);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create multiple records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="EntityNotFoundException">One or more requested ids do not exist.</exception>
    public virtual async Task<IEnumerable<TModel>> UpdateRangeAsync(IEnumerable<(int Id, TModel Model)> requests, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating {Count} records of type {EntityType}", requests.Count(), typeof(TEntity).Name);

            var requestsList = requests.ToList();
            var ids = requestsList.Select(r => r.Id).ToList();

            // Load all entities in one round-trip.
            var existingEntities = await _repository.FindManyAsync(ids, cancellationToken: cancellationToken);

            if (existingEntities.Count != requestsList.Count)
            {
                var foundIds = existingEntities.Select(e => e.Id).ToHashSet();
                var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();
                throw new EntityNotFoundException($"Records with ids {string.Join(", ", missingIds)} were not found");
            }

            // Index by id for fast lookup while applying each request.
            var entitiesDict = existingEntities.ToDictionary(e => e.Id);

            // Map each request's model onto the matching existing entity.
            foreach (var (id, model) in requestsList)
            {
                if (entitiesDict.TryGetValue(id, out var existingEntity))
                {
                    _mapper.Map(model, existingEntity);
                }
            }

            // Persist all updated entities in one round-trip.
            await _repository.UpdateRangeAsync(existingEntities, cancellationToken);

            var models = _mapper.Map<IEnumerable<TModel>>(existingEntities);
            _logger.LogInformation("Updated {Count} records of type {EntityType}", models.Count(), typeof(TEntity).Name);

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update multiple records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            var idList = ids.ToList();
            _logger.LogDebug("Deleting {Count} records of type {EntityType}", idList.Count, typeof(TEntity).Name);

            // Batch delete: one SELECT + one SaveChanges.
            bool result = await _repository.RemoveRangeAsync(idList, cancellationToken);

            _logger.LogInformation("Deleted {Count} records of type {EntityType}", idList.Count, typeof(TEntity).Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete multiple records of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving pagination info for type {EntityType}", typeof(TEntity).Name);

            var paginationInfo = await _repository.GetPaginationInfoAsync(parameters, cancellationToken);

            _logger.LogDebug("Pagination: total count {TotalCount}, total pages {TotalPages}",
                paginationInfo.TotalCount, paginationInfo.TotalPages);

            return paginationInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pagination info");
            throw;
        }
    }

    /// <summary>Synchronous, blocking equivalent of <see cref="GetAllAsync(string[], CancellationToken)"/>. Not used anywhere in this solution.</summary>
    [Obsolete("Use GetAllAsync instead")]
    public virtual IEnumerable<TModel> GetAll()
    {
        return GetAllAsync().GetAwaiter().GetResult();
    }

    /// <summary>Synchronous, blocking equivalent of <see cref="GetByIdAsync"/>. Not used anywhere in this solution.</summary>
    [Obsolete("Use GetByIdAsync instead")]
    public virtual TModel? GetById(int id)
    {
        return GetByIdAsync(id).GetAwaiter().GetResult();
    }

    /// <summary>Synchronous, blocking equivalent of <see cref="CreateAsync"/>. Not used anywhere in this solution.</summary>
    [Obsolete("Use CreateAsync instead")]
    public virtual TModel Create(TModel request)
    {
        return CreateAsync(request).GetAwaiter().GetResult();
    }

    /// <summary>Synchronous, blocking equivalent of <see cref="UpdateAsync(int, TModel, CancellationToken)"/>. Not used anywhere in this solution.</summary>
    [Obsolete("Use UpdateAsync instead")]
    public virtual TModel Update(int id, TModel request)
    {
        var result = UpdateAsync(id, request).GetAwaiter().GetResult();
        return result ?? throw new EntityNotFoundException("Update failed");
    }

    /// <summary>Synchronous, blocking equivalent of <see cref="DeleteAsync"/>. Not used anywhere in this solution.</summary>
    [Obsolete("Use DeleteAsync instead")]
    public virtual void Delete(int id)
    {
        DeleteAsync(id).GetAwaiter().GetResult();
    }
}
