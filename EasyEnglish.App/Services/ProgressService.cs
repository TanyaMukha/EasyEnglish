using EasyEnglish.App.Interfaces;
using EasyEnglish.App.Models;
using System.Threading.Tasks;
using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services;

public class ProgressService
{
    private readonly IStorageService _storageService;

    public ProgressService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<ProgressStats> GetProgressStats(DailyGoals goals)
    {
        var stats = await _storageService.GetAsync<ProgressStats>("progress_stats") ?? new ProgressStats();
        stats.DailyProgress.CompletedTests ??= new Dictionary<string, int>();
        stats.DailyProgress.IncorrectTests ??= new Dictionary<string, int>();

        // Calculate total progress based on goals
        int totalTests = goals.Goals.Where(g => g.Value.Enabled).Sum(g => g.Value.Count);
        int completedTests = stats.DailyProgress.CompletedTests.Values.Sum();
        stats.TotalProgress = totalTests > 0 ? (int)((completedTests / (double)totalTests) * 100) : 0;

        return stats;
    }

    public async Task UpdateProgress(string testType, bool isCorrect)
    {
        var stats = await _storageService.GetAsync<ProgressStats>("progress_stats") ?? new ProgressStats();
        stats.DailyProgress.CompletedTests ??= new Dictionary<string, int>();
        stats.DailyProgress.IncorrectTests ??= new Dictionary<string, int>();

        if (isCorrect)
        {
            stats.DailyProgress.CompletedTests[testType] = (stats.DailyProgress.CompletedTests.GetValueOrDefault(testType) + 1);
        }
        else
        {
            stats.DailyProgress.IncorrectTests[testType] = (stats.DailyProgress.IncorrectTests.GetValueOrDefault(testType) + 1);
        }

        await _storageService.SetAsync("progress_stats", stats);
    }
}
