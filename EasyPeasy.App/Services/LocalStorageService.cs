using EasyPeasy.Core.Interfaces.Storage;
using Microsoft.JSInterop;
using System.Text.Json;

namespace EasyPeasy.App.Services;

/// <summary>
/// <see cref="IStorageService"/> backed by MAUI's <see cref="Preferences"/> API — the real
/// implementation behind every <c>EasyPeasy.Core.Interfaces.Storage.IStorageService</c> consumer
/// in this app, including <c>EasyPeasy.Cache</c>'s cache services and this folder's
/// <see cref="RecentActivityService"/>/<see cref="StreakService"/>/<c>VoiceSettingsService</c>.
/// Values are JSON-serialized to a string before being handed to <see cref="Preferences"/>, so every
/// read deserializes a fresh object — callers never get back the same object reference twice.
/// </summary>
public class LocalStorageService : IStorageService
{
    /// <summary>Returns <c>default</c> (not an exception) if the key is missing, empty, or fails to deserialize.</summary>
    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = Preferences.Default.Get(key, string.Empty);
            var result = string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Preferences GetAsync error: {ex.Message}");
            return Task.FromResult<T?>(default);
        }
    }

    /// <summary>Serializes <paramref name="value"/> to JSON and stores it. Failures are logged, not thrown.</summary>
    public Task SetAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            Preferences.Default.Set(key, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Preferences SetAsync error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }
}