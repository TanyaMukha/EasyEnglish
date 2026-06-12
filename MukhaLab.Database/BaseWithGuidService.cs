using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MukhaLab.Database;

public class BaseWithGuidService<TEntity, TModel> : BaseService<TEntity, TModel>, IBaseWithGuidService<TModel>
    where TEntity : AbstractEntity, IGuidRecord
    where TModel : class, IGuidRecord
{
    /// <summary>
    /// Той самий репозиторій, що й у базовому класі, але з guid-методами.
    /// </summary>
    protected IBaseWithGuidRepository<TEntity> GuidRepository { get; }

    protected BaseWithGuidService(
        IBaseWithGuidRepository<TEntity> repository,
        IMapper mapper,
        ILogger<BaseWithGuidService<TEntity, TModel>> logger)
        : base(repository, mapper, logger)
    {
        GuidRepository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<TModel?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        var entity = await GuidRepository.FindAsync(guid, cancellationToken);
        return entity is null ? null : _mapper.Map<TModel>(entity);
    }

    public async Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
    {
        return await GuidRepository.CheckExistingGuidsAsync(guids, cancellationToken);
    }
}
