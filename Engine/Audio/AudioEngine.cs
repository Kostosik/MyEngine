using MyEngine.Diagnostics;
using Silk.NET.OpenAL;

namespace MyEngine.Audio;

/// <summary>
/// Обёртка над OpenAL: устройство, контекст, буферы, источники.
///
/// Инициализация вынесена в отдельный метод Initialize() — конструктор
/// не может бросить исключение. Если Initialize не был вызван или упал,
/// IsInitialized = false, и все методы работают как no-op.
///
/// Это позволяет опциональной подсистеме "аудио" не валить игру,
/// если на машине нет звуковой карты или драйвер занят.
/// </summary>
public sealed unsafe class AudioEngine : IDisposable
{
    private ALContext? _alc;
    private AL? _al;
    private Device* _device;
    private Context* _context;

    public bool IsInitialized { get; private set; }
    public string? DeviceName { get; private set; }

    /// <summary>
    /// Конструктор ничего не делает. Все рискованные операции — в Initialize().
    /// </summary>
    public AudioEngine() { }

    /// <summary>
    /// Открыть устройство, создать контекст, сделать его текущим.
    /// Может бросить исключение — вызывающий должен ловить (см. Safe.TryInit).
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized) return;

        _alc = ALContext.GetApi();
        _al = AL.GetApi();

        _device = _alc.OpenDevice("");
        if (_device == null)
            throw new InvalidOperationException("No audio device available");

        _context = _alc.CreateContext(_device, null);
        if (_context == null)
        {
            _alc.CloseDevice(_device);
            _device = null;
            throw new InvalidOperationException("Failed to create audio context");
        }

        _alc.MakeContextCurrent(_context);

        // Получение имени устройства — тоже может бросить, но это уже после
        // успешного создания контекста, так что не критично.
        try
        {
            DeviceName = _alc.GetContextProperty(_device, GetContextString.DeviceSpecifier);
        }
        catch
        {
            DeviceName = "(unknown)";
        }

        IsInitialized = true;
        Log.Info("[Audio]",$" Initialized: {DeviceName}");
    }

    // ============================================================
    // Listener — обычно позиция камеры
    // ============================================================

    public void SetListenerPosition(float x, float y, float z)
    {
        if (!IsInitialized) return;
        _al!.SetListenerProperty(ListenerVector3.Position, x, y, z);
    }

    public void SetListenerVelocity(float x, float y, float z)
    {
        if (!IsInitialized) return;
        _al!.SetListenerProperty(ListenerVector3.Velocity, x, y, z);
    }

    public void SetListenerOrientation(
        float fx, float fy, float fz,
        float ux, float uy, float uz)
    {
        if (!IsInitialized) return;
        float[] orientation = { fx, fy, fz, ux, uy, uz };
        fixed (float* p = orientation)
            _al!.SetListenerProperty(ListenerFloatArray.Orientation, p);
    }

    // ============================================================
    // Buffers
    // ============================================================

    internal uint GenBuffer()
    {
        if (!IsInitialized) return 0;
        return _al!.GenBuffer();
    }

    internal void DeleteBuffer(uint buffer)
    {
        if (!IsInitialized || buffer == 0) return;
        _al!.DeleteBuffer(buffer);
    }

    internal void UploadBuffer(uint buffer, AudioClip clip)
    {
        if (!IsInitialized || buffer == 0) return;

        BufferFormat format = (clip.IsStereo, clip.Is16Bit) switch
        {
            (false, false) => BufferFormat.Mono8,
            (false, true) => BufferFormat.Mono16,
            (true, false) => BufferFormat.Stereo8,
            (true, true) => BufferFormat.Stereo16,
        };

        fixed (byte* dataPtr = clip.Data)
        {
            _al!.BufferData(buffer, format, dataPtr, clip.Data.Length, clip.SampleRate);
        }
    }

    // ============================================================
    // Sources
    // ============================================================

    internal uint GenSource()
    {
        if (!IsInitialized) return 0;
        return _al!.GenSource();
    }

    internal void DeleteSource(uint source)
    {
        if (!IsInitialized || source == 0) return;
        _al!.DeleteSource(source);
    }

    internal void SetSourceBuffer(uint source, uint buffer)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
    }

    internal void SetSourceFloat(uint source, SourceFloat prop, float value)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SetSourceProperty(source, prop, value);
    }

    internal void SetSourceInt(uint source, SourceInteger prop, int value)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SetSourceProperty(source, prop, value);
    }

    internal void SetSourceBool(uint source, SourceBoolean prop, bool value)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SetSourceProperty(source, prop, value);
    }

    internal void SetSourceVector(uint source, SourceVector3 prop, float x, float y, float z)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SetSourceProperty(source, prop, x, y, z);
    }

    internal void GetSourceInt(uint source, GetSourceInteger prop, out int value)
    {
        if (!IsInitialized || source == 0) { value = 0; return; }
        _al!.GetSourceProperty(source, prop, out value);
    }

    internal void Play(uint source)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SourcePlay(source);
    }

    internal void Pause(uint source)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SourcePause(source);
    }

    internal void Stop(uint source)
    {
        if (!IsInitialized || source == 0) return;
        _al!.SourceStop(source);
    }

    // ============================================================
    // Dispose
    // ============================================================

    public void Dispose()
    {
        if (!IsInitialized) return;

        // Снять текущий контекст — иначе DestroyContext кинет исключение
        _alc!.MakeContextCurrent(null);

        if (_context != null) _alc.DestroyContext(_context);
        if (_device != null) _alc.CloseDevice(_device);

        _al?.Dispose();
        _alc.Dispose();

        _context = null;
        _device = null;
        _al = null;
        _alc = null;
        IsInitialized = false;
    }
}