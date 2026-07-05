using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface ITestCardService : IBaseService<TestCardModel>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    Task<IEnumerable<TestCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);
}
