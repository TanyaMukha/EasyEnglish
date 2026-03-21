using EasyEnglish.App.Services.Speech;

namespace EasyEnglish.App.Interfaces;

public interface IVoiceProvider
{
    Task<IReadOnlyList<LocaleInfo>> GetAllVoicesAsync();
    void InvalidateCache();
}
