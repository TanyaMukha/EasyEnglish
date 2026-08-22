using EasyPeasy.Core.Interfaces.Storage;

namespace EasyPeasy.App.Services;

/// <summary>
/// Removes local storage keys left behind by retired features. Call <see cref="RemoveObsoleteKeysAsync"/>
/// once on app startup; it is idempotent, so running it again (or with nothing left to clean) is a no-op.
/// To retire a feature's storage key in the future, add it to <see cref="ObsoleteKeys"/> instead of writing
/// a one-off migration.
/// </summary>
public class StorageMaintenanceService
{
    /// <summary>Storage keys from retired features, kept as a record of what/why until removed.</summary>
    private static readonly IReadOnlyDictionary<string, string> ObsoleteKeys = new Dictionary<string, string>
    {
        ["words_for_today"] = "Left by the retired global DailyWordsService flow (replaced by per-unit/course practice setup pages).",
    };

    private readonly IStorageService _storageService;

    public StorageMaintenanceService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    /// <summary>Removes every key listed in <see cref="ObsoleteKeys"/> from persistent storage.</summary>
    public async Task RemoveObsoleteKeysAsync()
    {
        foreach (var key in ObsoleteKeys.Keys)
        {
            await _storageService.RemoveAsync(key);
        }
    }
}
