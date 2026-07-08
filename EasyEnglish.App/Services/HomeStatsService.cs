using EasyEnglish.Core.Interfaces.Services;

namespace EasyEnglish.App.Services;

public class TodayReviewStats
{
    public int Reviewed { get; set; }
    public int Goal { get; set; }
}

public class LibraryOverview
{
    public int Courses { get; set; }
    public int Units { get; set; }
    public int Words { get; set; }
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
