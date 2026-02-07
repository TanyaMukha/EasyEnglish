using EasyEnglish.Core.Interfaces.Storage;
using Microsoft.JSInterop;
using System.Text.Json;

namespace EasyEnglish.App.Services;

public class LocalStorageService : IStorageService
{
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