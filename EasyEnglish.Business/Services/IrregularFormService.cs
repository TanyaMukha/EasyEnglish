using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;

namespace EasyEnglish.Services.Services;

public class IrregularFormService : BaseService<IrregularFormEntity ,IrregularFormModel>, IIrregularFormService
{
    public IrregularFormService(
        IIrregularFormRepository repository,
        IMapper mapper,
        ILogger<IrregularFormService> logger)
        : base(repository, mapper, logger)
    {
    }
}
