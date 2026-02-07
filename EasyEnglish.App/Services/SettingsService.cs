using EasyEnglish.App.Interfaces;
using EasyEnglish.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services;

public class SettingsService
{
    private readonly IStorageService _storageService;

    public SettingsService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<DailyGoals> GetSettingsAsync()
    {
        var settings = await _storageService.GetAsync<DailyGoals>("settings") ?? new DailyGoals();
        if (!settings.Goals.Any())
        {
            settings.Goals = new Dictionary<string, DailyGoal>
                {
                    { "select-translation", new DailyGoal { Enabled = true, Count = 10 } },
                    { "select-word", new DailyGoal { Enabled = true, Count = 10 } },
                    { "check-word", new DailyGoal { Enabled = true, Count = 10 } },
                    { "check-translation", new DailyGoal { Enabled = true, Count = 10 } },
                    { "input-word", new DailyGoal { Enabled = true, Count = 10 } }
                };
            await _storageService.SetAsync("settings", settings);
        }
        return settings;
    }

    public async Task<int> GetWordsPerDayAsync()
    {
        var count = await _storageService.GetAsync<int?>("words_per_day") ?? 10;
        return count;
    }

    public async Task<int> GetHardWordsPerDayAsync()
    {
        var count = await _storageService.GetAsync<int?>("hard_words_per_day") ?? 5;
        return count;
    }
}
