namespace EasyEnglish.App.Interfaces;

public interface IAudioService
{
    Task PlayAsync(byte[] audioData);
    void Stop();
}
