using EasyPeasy.App.Interfaces;
using EasyPeasy.App.Models;
using EasyPeasy.Core.Interfaces.Storage;

namespace EasyPeasy.App.Services;

/// <summary>Tracks the learner's daily-visit streak (consecutive calendar days with at least one visit).</summary>
public class StreakService
{
    private readonly IStorageService _storageService;

    public StreakService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    /// <summary>
    /// Call once per app visit (e.g. on the home page). Compares today against the last recorded
    /// visit date: more than 1 day ago (or never) resets the streak to 1; exactly yesterday
    /// increments it; today (already recorded) leaves the count unchanged. Always persists the
    /// updated streak, even when unchanged, and returns it.
    /// </summary>
    public async Task<StreakInfo> CheckAndUpdateStreak()
    {
        var streak = await _storageService.GetAsync<StreakInfo>("streak_info") ?? new StreakInfo();
        var today = DateTime.Today;

        if (streak.LastVisitDate == null || streak.LastVisitDate < today.AddDays(-1))
        {
            streak.CurrentStreak = 1;
        }
        else if (streak.LastVisitDate < today)
        {
            streak.CurrentStreak++;
        }

        if (streak.CurrentStreak > streak.HighestStreak)
        {
            streak.HighestStreak = streak.CurrentStreak;
        }

        streak.LastVisitDate = today;
        await _storageService.SetAsync("streak_info", streak);
        return streak;
    }
}
