using EasyEnglish.App.Interfaces;
using EasyEnglish.App.Models;
using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services;

public class StreakService
{
    private readonly IStorageService _storageService;

    public StreakService(IStorageService storageService)
    {
        _storageService = storageService;
    }

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
