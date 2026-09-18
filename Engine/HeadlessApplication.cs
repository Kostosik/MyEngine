using MyEngine.Audio;
using MyEngine.Coroutines;
using MyEngine.Diagnostics;
using MyEngine.Events;
using MyEngine.Memory;
using MyEngine.Rendering.RHI;
using MyEngine.Rendering.RHI.Null;
using MyEngine.Scenes;
using MyEngine.Threading;
using MyEngine.Timers;

namespace MyEngine;

/// <summary>
/// Headless Application — без окна, без GL, без ImGui.
/// Использует NullRenderer, все draw-вызовы игнорируются.
///
/// Для тестов, CI, серверной симуляции.
///
/// Использование в тестах:
///   using var app = new HeadlessApplication();
///   app.Initialize();
///   for (int i = 0; i < 60; i++) app.Tick(1f/60f);
///   app.Shutdown();
/// </summary>
public class HeadlessApplication : IDisposable
{
    private JobSystem _jobs = null!;
    private EventBus _events = null!;
    private Scheduler _scheduler = null!;
    private CoroutineRunner _coroutines = null!;
    private Profiler _profiler = null!;
    private Arena _frameArena = null!;
    private AudioEngine _audioEngine = null!;
    private AudioManager _audio = null!;
    private IRenderer _renderer = null!;

    public JobSystem Jobs => _jobs;
    public EventBus Events => _events;
    public Scheduler Scheduler => _scheduler;
    public CoroutineRunner Coroutines => _coroutines;
    public Profiler Profiler => _profiler;
    public Arena FrameArena => _frameArena;
    public AudioManager Audio => _audio;
    public IRenderer Renderer => _renderer;

    public int Width { get; }
    public int Height { get; }

    private bool _initialized;

    public HeadlessApplication(int width = 1280, int height = 720)
    {
        Width = width;
        Height = height;
    }

    public void Initialize()
    {
        if (_initialized) return;

        _renderer = new NullRenderer();
        _jobs = new JobSystem();
        _events = new EventBus();
        _scheduler = new Scheduler();
        _coroutines = new CoroutineRunner();
        _profiler = new Profiler();
        _frameArena = new Arena(1 * 1024 * 1024);
        _audioEngine = new AudioEngine();
        Safe.TryInit("Audio.Initialize", () => _audioEngine.Initialize());
        _audio = new AudioManager(_audioEngine);

        _initialized = true;

        Load();
    }

    /// <summary>Один тик логики. Не рисует.</summary>
    public void Tick(float dt)
    {
        if (!_initialized) Initialize();

        _frameArena.Reset();

        _profiler.BeginFrame();

        UpdateVariable(dt);
        _scheduler.Update(dt);
        _coroutines.Update(dt);
        Update(dt);

        _profiler.EndFrame();
    }

    /// <summary>Прогнать N кадров по dt. Удобно в тестах.</summary>
    public void RunFrames(int count, float dt = 1f / 60f)
    {
        for (int i = 0; i < count; i++) Tick(dt);
    }

    public void Shutdown()
    {
        if (!_initialized) return;

        Unload();

        Safe.Run("Audio.Dispose", () => _audio.Dispose());
        Safe.Run("AudioEngine.Dispose", () => _audioEngine.Dispose());
        _jobs.Dispose();
        _renderer.Dispose();

        _initialized = false;
    }

    protected virtual void Load() { }
    protected virtual void Update(float dt) { }
    protected virtual void UpdateVariable(float dt) { }
    protected virtual void Unload() { }

    public void Dispose() => Shutdown();
}