using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Presets;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;
using EasyEnglish.Data.Repositories;

namespace EasyEnglish.Services.Services;

public class UnitService : BaseWithGuidService<UnitEntity, UnitModel>, IUnitService
{
    private readonly IUnitRepository unitRepository;
    private readonly IWordService wordService;
    private readonly IExampleService exampleService;
    private readonly IIrregularFormService irregularFormService;
    private readonly IStudyCardService studyCardService;
    private readonly ITestCardService testCardService;

    public UnitService(
        IUnitRepository repository,
        IMapper mapper,
        ILogger<UnitService> logger,
        IWordService wordService,
        IExampleService exampleService,
        IIrregularFormService irregularFormService,
        IStudyCardService studyCardService,
        ITestCardService testCardService)
        : base(repository, mapper, logger)
    {
        this.unitRepository = repository;
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
        this.exampleService = exampleService ?? throw new ArgumentNullException(nameof(exampleService));
        this.irregularFormService = irregularFormService ?? throw new ArgumentNullException(nameof(irregularFormService));
        this.studyCardService = studyCardService ?? throw new ArgumentNullException(nameof(studyCardService));
        this.testCardService = testCardService ?? throw new ArgumentNullException(nameof(testCardService));
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

    public async Task<IEnumerable<WordModel>> GetWordsAsync(int unitId, string[]? includes = null)
    {
        return await this.wordService.GetByUnitAsync(unitId, includes);
    }

    public async Task<IEnumerable<ExampleModel>> GetExamplesAsync(int unitId)
    {
        return await this.exampleService.GetByUnitAsync(unitId);
    }

    /// <summary>
    /// Оновлює юніт разом з дочірніми елементами (Words/Examples/IrregularForms/StudyCards/TestCards),
    /// зіставляючи їх з наявними в базі за <see cref="MukhaLab.Database.IGuidRecord.RecordGuid"/> замість
    /// сліпого EF-каскаду за Id. Елемент, чий RecordGuid уже є серед дочірніх елементів наявного юніта,
    /// отримує реальний Id цього рядка (і буде оновлений на місці); новий RecordGuid — Id лишається 0
    /// (буде вставлений як новий рядок). FK (UnitId/WordId) виправляти вручну не треба — EF сам
    /// підставляє їх при збереженні графа через навігаційні колекції.
    /// </summary>
    /// <param name="deleteMissing">
    /// Якщо true — дочірні елементи, чийого RecordGuid немає серед вхідних, видаляються з бази
    /// (сувора синхронізація). Якщо false — такі елементи лишаються без змін (тільки додавання/оновлення).
    /// </param>
    public async Task<UnitModel> ReconcileAndUpdateAsync(UnitModel incoming, bool deleteMissing, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(incoming.Id, UnitIncludes.Full, cancellationToken)
            ?? throw new ArgumentException($"Юніт з ID {incoming.Id} не знайдено");

        var existingWordsByGuid = (existing.Words ?? []).ToDictionary(w => w.RecordGuid);
        var orphanExampleIds = new List<int>();

        ReconcileIds(incoming.Words, existing.Words ?? []);
        foreach (var word in incoming.Words ?? [])
        {
            if (!existingWordsByGuid.TryGetValue(word.RecordGuid, out var existingWord))
                continue; // нове слово — його приклади теж нові, звіряти нема з чим

            ReconcileIds(word.Examples, existingWord.Examples ?? []);
            if (deleteMissing)
                orphanExampleIds.AddRange(FindOrphanIds(word.Examples, existingWord.Examples ?? []));
        }

        ReconcileIds(incoming.IrregularForms, existing.IrregularForms ?? []);
        ReconcileIds(incoming.StudyCards, existing.StudyCards ?? []);
        ReconcileIds(incoming.TestCards, existing.TestCards ?? []);

        if (deleteMissing)
        {
            await DeleteOrphansAsync(wordService, FindOrphanIds(incoming.Words, existing.Words ?? []), cancellationToken);
            await DeleteOrphansAsync(exampleService, orphanExampleIds, cancellationToken);
            await DeleteOrphansAsync(irregularFormService, FindOrphanIds(incoming.IrregularForms, existing.IrregularForms ?? []), cancellationToken);
            await DeleteOrphansAsync(studyCardService, FindOrphanIds(incoming.StudyCards, existing.StudyCards ?? []), cancellationToken);
            await DeleteOrphansAsync(testCardService, FindOrphanIds(incoming.TestCards, existing.TestCards ?? []), cancellationToken);
        }

        return await UpdateAsync(incoming.Id, incoming, cancellationToken);
    }

    private static void ReconcileIds<T>(IList<T>? incoming, IEnumerable<T> existing)
        where T : AbstractModel, IGuidRecord
    {
        if (incoming is null) return;

        var existingIdsByGuid = existing.ToDictionary(e => e.RecordGuid, e => e.Id);
        foreach (var item in incoming)
            if (existingIdsByGuid.TryGetValue(item.RecordGuid, out var existingId))
                item.Id = existingId;
    }

    private static List<int> FindOrphanIds<T>(IList<T>? incoming, IEnumerable<T> existing)
        where T : AbstractModel, IGuidRecord
    {
        var incomingGuids = (incoming ?? []).Select(i => i.RecordGuid).ToHashSet();
        return existing.Where(e => !incomingGuids.Contains(e.RecordGuid)).Select(e => e.Id).ToList();
    }

    private static async Task DeleteOrphansAsync<TModel>(IBaseService<TModel> service, List<int> orphanIds, CancellationToken cancellationToken)
        where TModel : class
    {
        if (orphanIds.Count > 0)
            await service.DeleteRangeAsync(orphanIds, cancellationToken);
    }
}