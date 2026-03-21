using AutoMapper;
using EasyEnglish.Services;
using Microsoft.Extensions.Logging;

namespace MukhaLab.Database;

public class BaseWithGuidService<TEntity, TModel> : BaseService<TEntity, TModel>, IBaseWithGuidService<TModel>
    where TEntity : AbstractEntity, IGuidRecord
    where TModel : class, IGuidRecord
{
    protected new readonly IBaseWithGuidRepository<TEntity> _repository;
    protected new readonly IMapper _mapper;
    protected new readonly ILogger<BaseWithGuidService<TEntity, TModel>> _logger;

    protected BaseWithGuidService(
        IBaseWithGuidRepository<TEntity> repository,
        IMapper mapper,
        ILogger<BaseWithGuidService<TEntity, TModel>> logger)
        : base(repository, mapper, logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TModel?> GetByGuidAsync(Guid guid)
    {
        return _mapper.Map<TModel>(await _repository.FindAsync(guid));
    }

    public async Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids)
    {
        return await _repository.CheckExistingGuidsAsync(guids);
    }
}
