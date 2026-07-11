using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MukhaLab.Database;

/// <summary>
/// <see cref="BaseService{TEntity, TModel}"/> extended with <see cref="IGuidRecord.RecordGuid"/>-based
/// lookups, mirroring <see cref="BaseWithGuidRepository{T, TContext}"/> at the service layer.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must derive from <see cref="AbstractEntity"/> and implement <see cref="IGuidRecord"/>.</typeparam>
/// <typeparam name="TModel">The model type. Must implement <see cref="IGuidRecord"/>.</typeparam>
public class BaseWithGuidService<TEntity, TModel> : BaseService<TEntity, TModel>, IBaseWithGuidService<TModel>
    where TEntity : AbstractEntity, IGuidRecord
    where TModel : class, IGuidRecord
{
    /// <summary>
    /// The same repository instance as the base class's <c>_repository</c>, exposed as
    /// <see cref="IBaseWithGuidRepository{T}"/> so its GUID-based methods are reachable here.
    /// </summary>
    protected IBaseWithGuidRepository<TEntity> GuidRepository { get; }

    /// <inheritdoc cref="BaseService{TEntity, TModel}(IBaseRepository{TEntity}, IMapper, ILogger{BaseService{TEntity, TModel}})"/>
    protected BaseWithGuidService(
        IBaseWithGuidRepository<TEntity> repository,
        IMapper mapper,
        ILogger<BaseWithGuidService<TEntity, TModel>> logger)
        : base(repository, mapper, logger)
    {
        GuidRepository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public async Task<TModel?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        var entity = await GuidRepository.FindAsync(guid, cancellationToken);
        return entity is null ? null : _mapper.Map<TModel>(entity);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
    {
        return await GuidRepository.CheckExistingGuidsAsync(guids, cancellationToken);
    }
}
