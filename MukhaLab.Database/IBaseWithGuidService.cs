using EasyEnglish.Core.Interfaces.Services;

namespace MukhaLab.Database;

public interface IBaseWithGuidService<TModel> : IBaseService<TModel>
    where TModel : class, IGuidRecord
{
    Task<TModel?> GetByGuidAsync(Guid guid);
    Task<IEnumerable<Guid>> GetExistingGuidsAsync(IEnumerable<Guid> guids);
}
