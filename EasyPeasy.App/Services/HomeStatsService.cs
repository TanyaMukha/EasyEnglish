using EasyPeasy.Core.Interfaces.Services;

namespace EasyPeasy.App.Services;

/// <summary>Today's review progress for the home page's progress ring.</summary>
public class TodayReviewStats
{
    /// <summary>Words/irregular forms/study cards/test cards reviewed today (UTC), summed across all kinds.</summary>
    public int Reviewed { get; set; }

    /// <summary>Target for today, currently always <see cref="HomeStatsService.DailyGoal"/>.</summary>
    public int Goal { get; set; }
}

/// <summary>Total counts across the whole library, for the home page's overview cards.</summary>
public class LibraryOverview
{
    public int Courses { get; set; }
    public int Units { get; set; }
    public int Words { get; set; }

    /// <summary>Study cards plus test cards combined.</summary>
    public int Cards { get; set; }
}

/// <summary>
/// Home-page stats derived from real data already in the database — no separate
/// "progress" bookkeeping to keep in sync. "Reviewed today" counts any word, irregular
/// form, study card or test card whose LastReviewDate falls on today (UTC), since every
/// practice session already sets that field via <see cref="WordRatingCalculator"/>.
/// </summary>
public class HomeStatsService
{
    /// <summary>Fixed daily review target for the progress ring. Not user-configurable yet.</summary>
    public const int DailyGoal = 20;

    private readonly IWordService _wordService;
    private readonly IIrregularFormService _irregularFormService;
    private readonly IStudyCardService _studyCardService;
    private readonly ITestCardService _testCardService;
    private readonly ICourseService _courseService;
    private readonly IUnitService _unitService;

    public HomeStatsService(
        IWordService wordService,
        IIrregularFormService irregularFormService,
        IStudyCardService studyCardService,
        ITestCardService testCardService,
        ICourseService courseService,
        IUnitService unitService)
    {
        _wordService = wordService;
        _irregularFormService = irregularFormService;
        _studyCardService = studyCardService;
        _testCardService = testCardService;
        _courseService = courseService;
        _unitService = unitService;
    }

    /// <summary>Counts everything reviewed since midnight UTC, across all 4 learnable-item kinds.</summary>
    public async Task<TodayReviewStats> GetTodayReviewedCountAsync()
    {
        var since = DateTime.UtcNow.Date;

        var counts = await Task.WhenAll(
            _wordService.CountReviewedSinceAsync(since),
            _irregularFormService.CountReviewedSinceAsync(since),
            _studyCardService.CountReviewedSinceAsync(since),
            _testCardService.CountReviewedSinceAsync(since));

        return new TodayReviewStats
        {
            Reviewed = counts.Sum(),
            Goal = DailyGoal,
        };
    }

    /// <summary>Unfiltered totals — every course/unit/word/card in the database, regardless of review state.</summary>
    public async Task<LibraryOverview> GetLibraryOverviewAsync()
    {
        var counts = await Task.WhenAll(
            _courseService.CountAsync(),
            _unitService.CountAsync(),
            _wordService.CountAsync(),
            _studyCardService.CountAsync(),
            _testCardService.CountAsync());

        return new LibraryOverview
        {
            Courses = counts[0],
            Units   = counts[1],
            Words   = counts[2],
            Cards   = counts[3] + counts[4],
        };
    }
}
