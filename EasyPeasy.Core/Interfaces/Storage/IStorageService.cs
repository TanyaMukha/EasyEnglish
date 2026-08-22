namespace EasyPeasy.Core.Interfaces.Storage;

/// <summary>
/// Simple key/value persistent storage abstraction (e.g. backed by MAUI <c>Preferences</c>/<c>SecureStorage</c>),
/// used by the <c>Interfaces.Cache</c> services to survive app restarts.
/// </summary>
public interface IStorageService
{
    /// <summary>Reads and deserializes the value stored under <paramref name="key"/>.</summary>
    Task<T> GetAsync<T>(string key);

    /// <summary>Serializes and writes <paramref name="value"/> under <paramref name="key"/>.</summary>
    Task SetAsync<T>(string key, T value);

    /// <summary>Removes the value stored under <paramref name="key"/>, if any.</summary>
    Task RemoveAsync(string key);
}
