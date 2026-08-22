using EasyPeasy.App.Interfaces;
using Plugin.Maui.Audio;

namespace EasyPeasy.App.Services;

/// <summary>
/// Plays a single in-memory audio clip (e.g. a word's stored pronunciation) via
/// <see cref="Plugin.Maui.Audio"/>. Not the same subsystem as <c>Services/Speech/*</c>
/// (text-to-speech) — this plays pre-recorded bytes, TTS synthesizes speech from text.
/// </summary>
public class AudioService : IAudioService, IDisposable
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    /// <summary>Stops any currently-playing clip and starts playing <paramref name="audioData"/>.</summary>
    public async Task PlayAsync(byte[] audioData)
    {
        Stop();

        var stream = new MemoryStream(audioData);
        _player = _audioManager.CreatePlayer(stream);
        _player.Play();
    }

    /// <summary>Stops and disposes the current player, if any. Safe to call when nothing is playing.</summary>
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
