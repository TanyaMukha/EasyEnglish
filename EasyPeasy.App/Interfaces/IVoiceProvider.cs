using EasyPeasy.App.Services.Speech;

namespace EasyPeasy.App.Interfaces;

/// <summary>Lists the voices available for text-to-speech on this platform. See <see cref="EasyPeasy.App.Services.Speech.MauiVoiceProvider"/> for the MAUI implementation.</summary>
public interface IVoiceProvider
{
    /// <summary>Returns every available voice, typically cached after the first call.</summary>
    Task<IReadOnlyList<LocaleInfo>> GetAllVoicesAsync();
    /// <summary>Clears any cached voice list so the next <see cref="GetAllVoicesAsync"/> call re-queries the platform.</summary>
    void InvalidateCache();
}
