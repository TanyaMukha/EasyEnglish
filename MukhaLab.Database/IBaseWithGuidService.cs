namespace MukhaLab.Database;

public interface IBaseWithGuidService<TModel> : IBaseService<TModel>
    where TModel : class, IGuidRecord
{
    Task<TModel?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default);
}
