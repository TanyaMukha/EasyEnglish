using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services;

public class LastVisitedUnit
{
    public int CourseId { get; set; }
    public int UnitId { get; set; }
    public string UnitTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime VisitedAt { get; set; }
}

/// <summary>Tracks the last unit the learner opened, so the home page can offer a "continue" shortcut.</summary>
public class RecentActivityService
{
    private const string StorageKey = "last_visited_unit";

    private readonly IStorageService _storageService;

    public RecentActivityService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public Task RecordUnitVisitAsync(int courseId, int unitId, string unitTitle, string courseTitle)
    {
        return _storageService.SetAsync(StorageKey, new LastVisitedUnit
        {
            CourseId = courseId,
            UnitId = unitId,
            UnitTitle = unitTitle,
            CourseTitle = courseTitle,
            VisitedAt = DateTime.UtcNow,
        });
    }

    public Task<LastVisitedUnit?> GetLastVisitedAsync() => _storageService.GetAsync<LastVisitedUnit?>(StorageKey);
}
