using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;
using MukhaLab.Database;
using EasyEnglish.Data.Repositories;

namespace EasyEnglish.Services.Services;

public class UnitService : BaseWithGuidService<UnitEntity, UnitModel>, IUnitService
{
    private readonly IUnitRepository unitRepository;
    private readonly IWordService wordService;

    public UnitService(
        IUnitRepository repository,
        IMapper mapper,
        ILogger<UnitService> logger,
        IWordService wordService)
        : base(repository, mapper, logger)
    {
        this.unitRepository = repository;
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
    }

    public async Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId)
    {
        _logger.LogDebug("Завантаження карток юнітів для курсу {CourseId}", courseId);
        return await this.unitRepository.GetUnitCardsAsync(courseId);
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

        return await this.wordService.GetAllAsync(parameters);
    }
}