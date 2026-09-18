using Silk.NET.OpenAL;

namespace MyEngine.Audio;

public sealed class AudioSource : IDisposable
{
    private readonly AudioEngine _engine;
    internal uint Source { get; }

    public float Volume
    {
        get => _volume;
        set { _volume = value; _engine.SetSourceFloat(Source, SourceFloat.Gain, value); }
    }
    private float _volume = 1f;

    public float Pitch
    {
        get => _pitch;
        set { _pitch = value; _engine.SetSourceFloat(Source, SourceFloat.Pitch, value); }
    }
    private float _pitch = 1f;


    public bool Loop
    {
        get => _loop;
        set { _loop = value; _engine.SetSourceBool(Source, SourceBoolean.Looping, value); }
    }
    private bool _loop;

    public AudioSource(AudioEngine engine)
    {
        _engine = engine;
        if (!engine.IsInitialized)
        {
            Source = 0;
            return;
        }
        Source = engine.GenSource();
        engine.SetSourceFloat(Source, SourceFloat.Gain, 1f);
        engine.SetSourceFloat(Source, SourceFloat.Pitch, 1f);
    }

    public void SetPosition(float x, float y, float z = 0)
        => _engine.SetSourceVector(Source, SourceVector3.Position, x, y, z);

    public void Play(AudioClipResource clip)
    {
        _engine.SetSourceBuffer(Source, clip.Buffer);
        _engine.Play(Source);
    }

    public void Stop() => _engine.Stop(Source);
    public void Pause() => _engine.Pause(Source);

    public bool IsPlaying
    {
        get
        {
            _engine.GetSourceInt(Source, GetSourceInteger.SourceState, out int state);
            return (SourceState)state == SourceState.Playing;
        }
    }

    public void Dispose()
    {
        if (Source != 0) _engine.DeleteSource(Source);
    }
}