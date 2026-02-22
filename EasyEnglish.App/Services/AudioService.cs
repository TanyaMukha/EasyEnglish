using EasyEnglish.App.Interfaces;
using Plugin.Maui.Audio;

namespace EasyEnglish.App.Services;

public class AudioService : IAudioService, IDisposable
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task PlayAsync(byte[] audioData)
    {
        Stop();

        var stream = new MemoryStream(audioData);
        _player = _audioManager.CreatePlayer(stream);
        _player.Play();
    }

    public void Stop()
    {
        if (_player is not null)
        {
            _player.Stop();
            _player.Dispose();
            _player = null;
        }
    }

    public void Dispose() => Stop();
}
