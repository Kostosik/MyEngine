namespace MyEngine.Audio;

public sealed class AudioClipResource : IDisposable
{
    private readonly AudioEngine _engine;
    public AudioClip Clip { get; }
    internal uint Buffer { get; }
    public string Name { get; }

    public AudioClipResource(AudioEngine engine, AudioClip clip, string name)
    {
        _engine = engine;
        Clip = clip;
        Name = name;
        Buffer = engine.GenBuffer();
        engine.UploadBuffer(Buffer, clip);
    }

    public void Dispose() => _engine.DeleteBuffer(Buffer);
}