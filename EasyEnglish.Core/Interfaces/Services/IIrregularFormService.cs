using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IIrregularFormService : IBaseService<IrregularFormModel>
{
    Task<IEnumerable<IrregularFormModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> forms);
}
