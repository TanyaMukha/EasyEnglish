using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services;

/// <summary>Snapshot of the most recently opened unit, persisted so the home page can offer a "continue" shortcut across app restarts.</summary>
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

    /// <summary>Overwrites the stored "last visited unit" with the given one, timestamped now.</summary>
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

    /// <summary>Returns the last visited unit, or <c>null</c> if none has been recorded yet.</summary>
    public Task<LastVisitedUnit?> GetLastVisitedAsync() => _storageService.GetAsync<LastVisitedUnit?>(StorageKey);
}
