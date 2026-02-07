using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Storage;
using EasyEnglish.Core.Models;
using EasyEnglish.App.Models;

namespace EasyEnglish.App.Services;

public class DailyWordsService
{
    private readonly IWordService _wordService;
    private readonly IUnitService _unitService;
    private readonly IStorageService _storageService;

    // In-memory кеш
    private WordsForTodayResult? _cachedResult;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public DailyWordsService(
        IWordService wordService,
        IUnitService unitService,
        IStorageService storageService)
    {
        _wordService = wordService;
        _unitService = unitService;
        _storageService = storageService;
    }

    public async Task<WordsForTodayResult> GetWordsForToday()
    {
        // Якщо є в кеші — миттєве повернення
        if (_cachedResult != null)
            return _cachedResult;

        await _lock.WaitAsync();
        try
        {
            // Double-check після придбання lock
            if (_cachedResult != null)
                return _cachedResult;

            _cachedResult = await FetchWordsForTodayAsync();
            return _cachedResult;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetWordsForToday(List<int> wordIds, int? unitId = null)
    {
        var wordIdSet = new WordIdsForToday { WordIds = wordIds, UnitId = unitId };

        // Зберегти ID в Preferences
        await _storageService.SetAsync("words_for_today", wordIdSet);

        // Оновити кеш свіжими даними з БД
        _cachedResult = await FetchWordsForTodayAsync();
    }

    // Очистити кеш без видалення з Preferences
    // Використовується коли дані в БД змінились
    public void ClearCache()
    {
        _cachedResult = null;
    }

    // Очистити кеш і видалити з Preferences
    // Використовується коли потрібно скинути дані назавжди
    public async Task ClearAsync()
    {
        _cachedResult = null;
        await _storageService.SetAsync<WordIdsForToday?>("words_for_today", null);
    }

    private async Task<WordsForTodayResult> FetchWordsForTodayAsync()
    {
        var wordIdSet = await _storageService.GetAsync<WordIdsForToday>("words_for_today")
            ?? new WordIdsForToday();

        var words = await _wordService.GetByIdsAsync(wordIdSet.WordIds);

        var unit = wordIdSet.UnitId.HasValue
            ? await _unitService.GetByIdAsync(wordIdSet.UnitId.Value)
            : new UnitModel();

        return new WordsForTodayResult
        {
            Words = words,
            Unit = unit
        };
    }
}