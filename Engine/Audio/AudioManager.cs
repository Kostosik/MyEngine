using MyEngine.Diagnostics;

namespace MyEngine.Audio;

public sealed class AudioManager : IDisposable
{
    private readonly AudioEngine _engine;
    private readonly Dictionary<string, AudioClipResource> _clips = new();
    private readonly List<AudioSource> _pool = new();
    private readonly int _maxConcurrent = 32;

    public bool Enabled => _engine.IsInitialized;
    public float MasterVolume { get; set; } = 1f;

    public AudioManager(AudioEngine engine)
    {
        _engine = engine;
        if (!engine.IsInitialized)
        {
            Log.Info("[Audio]"," Manager running in silent mode");
            return;
        }
        for (int i = 0; i < _maxConcurrent; i++)
            _pool.Add(new AudioSource(engine));
    }

    public AudioClipResource? Load(string name, string path)
    {
        if (!Enabled) return null;
        if (_clips.TryGetValue(name, out var existing)) return existing;

        var clip = WavLoader.Load(path);
        var resource = new AudioClipResource(_engine, clip, name);
        _clips[name] = resource;
        Log.Info("[Audio]",$" Loaded '{name}' ({clip.DurationSeconds:F2}s)");
        return resource;
    }

    /// <summary>Найти свободный source и проиграть.</summary>
    public AudioSource? Play(string name, float volume = 1f, bool loop = false)
    {
        if (!Enabled) return null;
        if (!_clips.TryGetValue(name, out var clip)) return null;

        var source = GetFreeSource();
        if (source == null) return null;

        source.Volume = volume * MasterVolume;
        source.Loop = loop;
        source.Play(clip);
        return source;
    }

    public AudioSource? PlayAt(string name, float x, float y, float volume = 1f)
    {
        var src = Play(name, volume);
        src?.SetPosition(x, y, 0);
        return src;
    }

    private AudioSource? GetFreeSource()
    {
        foreach (var s in _pool)
            if (!s.IsPlaying) return s;
        return null; // всё занято
    }

    public void Dispose()
    {
        foreach (var s in _pool) s.Dispose();
        foreach (var c in _clips.Values) c.Dispose();
    }
}