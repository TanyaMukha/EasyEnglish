using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;

namespace EasyEnglish.Services.Services;

public class UnitService : BaseService<UnitEntity, UnitModel>, IUnitService
{
    private readonly IWordService wordService;

    public UnitService(
        IUnitRepository repository,
        IMapper mapper,
        ILogger<UnitService> logger,
        IWordService wordService)
        : base(repository, mapper, logger)
    {
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
    }

    public async Task<IEnumerable<WordModel>> GetWordsAsync(int unitId)
    {
        QueryParameters parameters = new QueryParameters
        {
            Filters = new List<FilterParameter>
            {
                new FilterParameter
                {
                    Field = "UnitId",
                    Operation = FilterOperation.Equal,
                    DataType = FilterDataType.Integer,
                    Value = unitId
                }
            }
        };

        return await this.wordService.GetAllAsync(parameters, true);
    }
}