using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
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

    public async Task<IEnumerable<UnitModel>> GetByCourseAsync(int courseId)
    {
        var entities = await this.unitRepository.GetByCourseAsync(courseId);
        return _mapper.Map<IEnumerable<UnitModel>>(entities);
    }

    public async Task<IEnumerable<WordModel>> GetWordsAsync(int unitId)
    {
        return await this.wordService.GetByUnitAsync(unitId);
    }
}