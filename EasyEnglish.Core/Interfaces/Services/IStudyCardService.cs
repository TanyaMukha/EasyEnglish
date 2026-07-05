using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IStudyCardService : IBaseService<StudyCardModel>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId);

    Task<IEnumerable<StudyCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards);
}