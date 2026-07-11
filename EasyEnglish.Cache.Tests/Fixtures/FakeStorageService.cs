using System.Text.Json;
using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.Cache.Tests.Fixtures;

/// <summary>
/// In-memory <see cref="IStorageService"/> double that round-trips values through JSON, like a real
/// Preferences/SecureStorage-backed implementation would — deliberately not just holding onto the
/// same object reference, since a naive in-memory fake would mask reference-aliasing bugs that a
/// real (serializing) storage backend can't have.
/// </summary>
public class FakeStorageService : IStorageService
{
    private readonly Dictionary<string, string> _values = new();

    public Task<T> GetAsync<T>(string key)
    {
        if (_values.TryGetValue(key, out var json))
            return Task.FromResult(JsonSerializer.Deserialize<T>(json)!);

        return Task.FromResult<T>(default!);
    }

    public Task SetAsync<T>(string key, T value)
    {
        _values[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _values.Remove(key);
        return Task.CompletedTask;
    }
}
