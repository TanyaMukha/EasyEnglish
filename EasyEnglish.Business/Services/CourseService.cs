using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

public class CourseService : BaseWithGuidService<CourseEntity, CourseModel>, ICourseService
{
    private readonly IUnitService unitService;
    private readonly IWordService wordService;

    public CourseService(
        ICourseRepository course,
        IWordService wordService,
        IMapper mapper,
        ILogger<CourseService> logger,
        IUnitService unitService)
        : base(course, mapper, logger)
    {
        this.unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
    }

    public async Task<IEnumerable<UnitModel>> GetUnitsAsync(int courseId)
    {
        return await this.unitService.GetByCourseAsync(courseId);
    }

    public async Task<IEnumerable<WordModel>> GetWordsAsync(int courseId, int? unitId = null, LearningSelectionOptions? options = null)
    {
        options ??= new LearningSelectionOptions();

        IEnumerable<WordModel> words = await this.wordService.GetForLearningAsync(courseId, unitId, options);

        if (options.ShuffleWords)
        {
            words = words.OrderBy(x => Random.Shared.Next());
        }

        return words;
    }
}