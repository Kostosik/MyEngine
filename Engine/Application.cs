using MyEngine.Assets;
using MyEngine.Audio;
using MyEngine.Coroutines;
using MyEngine.Diagnostics;
using MyEngine.Diagnostics.Console;
using MyEngine.Diagnostics.Tools;
using MyEngine.Effects;
using MyEngine.Events;
using MyEngine.GameFlow;
using MyEngine.InputEngine;
using MyEngine.Memory;
using MyEngine.Rendering;
using MyEngine.Rendering.RHI;
using MyEngine.Scenes;
using MyEngine.Services;
using MyEngine.Threading;
using MyEngine.Time;
using MyEngine.Timers;
using MyEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace MyEngine;

public abstract class Application : IDisposable
{
    private readonly IWindow _window;
    private GL _gl = null!;
    private IInputContext _inputCtx = null!;
    private ImGuiLayer _imgui = null!;
    private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
    private double _lastTime;
    private double _accumulator;

    private const double FixedDt = 1.0 / 60.0;

    private DebugOverlay _debug = null!;
    public DebugOverlay Debug => _debug;
    public GL GL => _gl;
    public Input Input { get; private set; } = null!;
    public int Width => _window.Size.X;
    public int Height => _window.Size.Y;

    protected Application(string title = "My Engine", int width = 1280, int height = 720)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(width, height),
            Title = title,
            VSync = true,
            API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Core,
                ContextFlags.ForwardCompatible,
                new APIVersion(3, 3))
        };
        _window = Window.Create(options);
        _window.FramesPerSecond = 144;
        _window.Load += OnLoad;
        _window.Render += OnFrame;
        _window.Closing += OnClosing;
    }

    public void Run() => _window.Run();
    public void RequestClose() => _window.Close();

    private JobSystem _jobs = null!;
    public JobSystem Jobs => _jobs;

    private ResourceManager _resources = null!;
    protected ResourceManager Resources => _resources;

    private JobProfiler _jobProfiler = null!;
    private PerfTimelineWindow _timelineWindow = null!;
    public PerfTimelineWindow TimelineWindow => _timelineWindow;

    private Scheduler _scheduler = new();
    public Scheduler Scheduler => _scheduler;

    private Arena _frameArena = null!;
    protected Arena FrameArena => _frameArena;

    private SceneManager _scenes = null!;
    public SceneManager Scenes => _scenes;

    private AssetManifest _assets = new();
    public AssetManifest Assets => _assets;

    public ConsoleSystem? Console => Debug.Get<ConsoleTool>()?.Console;
    private Profiler _profiler = null!;
    public Profiler Profiler => _profiler;

    private ProfilerWindow _profilerWindow = null!;
    public ProfilerWindow ProfilerWindow => _profilerWindow;
    private GameStateMachine _gameStateMachine = new();
    public GameStateMachine AppGameStateMachine => _gameStateMachine;
    private EventBus _events = null!;
    public EventBus Events => _events;

    private UIRoot _ui = null!;
    public UIRoot UI => _ui;
    private UIRoundedRenderer _uiRounded = null!;
    public UIRoundedRenderer UIRounded => _uiRounded;

    public SpriteBatch Batch { get; set; } = null!;
    public Font Font { get; set; } = null!;
    public ParticleSystem Particles { get; set; } = null!;
    public Camera2D Camera { get; } = new();

    private FatalErrorOverlay _fatalError = new();
    protected FatalErrorOverlay FatalError => _fatalError;

    private AudioEngine _audioEngine = null!;
    private AudioManager _audio = null!;
    protected AudioManager Audio => _audio;
    protected AudioEngine AudioEngine => _audioEngine;

    private CoroutineRunner _coroutines = null!;
    public CoroutineRunner Coroutines => _coroutines;


    private OpenGLRenderer _renderer = null!;
    public IRenderer Renderer => _renderer;
    public OpenGLRenderer GLRenderer => _renderer;

    private void OnLoad()
    {
        InitializeLogger();
        InitializeTimeAndArgs();
        InitializeGraphics();
        InitializeCoreServices();
        InitializeAudio();
        RegisterServices();
        InitializeDebugTools();

        _clock.Restart();
        _lastTime = _clock.Elapsed.TotalSeconds;
        _assets.Load(Path.Combine(AppContext.BaseDirectory, "Assets", "Manifest.json"));

        Load();
    }

    private void InitializeLogger()
    {
        ErrorLogger.Initialize(Path.Combine(AppContext.BaseDirectory, "logs", "errors.log"));
    }

    private void InitializeTimeAndArgs()
    {
        TimeEngine.TargetFps = _window.FramesPerSecond is > 0
            ? (int)_window.FramesPerSecond
            : 60;
        CommandLine.Initialize(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

    private void InitializeGraphics()
    {
        _gl = _window.CreateOpenGL();
        _uiRounded = new UIRoundedRenderer(_gl);
        _renderer = new OpenGLRenderer(_gl);
        _inputCtx = _window.CreateInput();
        Input = new Input(_inputCtx);
        _imgui = new ImGuiLayer(_gl, _window, _inputCtx);
        _ui = new UIRoot();
    }

    private void InitializeCoreServices()
    {
        _jobs = new JobSystem();
        _jobProfiler = new JobProfiler(Environment.CurrentManagedThreadId);
        _jobs.AttachProfiler(_jobProfiler);
        _timelineWindow = new PerfTimelineWindow(_jobProfiler, _jobs);
        _resources = new ResourceManager(_gl, _jobs);
        _frameArena = new Arena(4 * 1024 * 1024);
        _scheduler = new Scheduler();
        _coroutines = new CoroutineRunner();
        _events = new EventBus();
        _profiler = new Profiler();
        _profilerWindow = new ProfilerWindow(_profiler);
        _scenes = new SceneManager();
        _scenes.Attach(this);
    }

    private void InitializeAudio()
    {
        _audioEngine = new AudioEngine();
        Safe.TryInit("Audio.Initialize", () => _audioEngine.Initialize());
        _audio = new AudioManager(_audioEngine);
    }

    private void RegisterServices()
    {
        ServicesEngine.Register(_audio);
        ServicesEngine.Register(_events);
        ServicesEngine.Register(_jobs);
        ServicesEngine.Register(_resources);
        ServicesEngine.Register(_scheduler);
        ServicesEngine.Register(_coroutines);
        ServicesEngine.Register(_profiler);
    }

    private void InitializeDebugTools()
    {
        _debug = new DebugOverlay();
        _debug.Register(new ConsoleTool());
        _debug.Register(new WatchTool());
        _debug.Register(new DebugFlagsTool());
        _debug.Register(new ProfilerTool(_profiler, _profilerWindow));
        _debug.Register(new TimelineTool(_timelineWindow));
#if DEBUG
        _debug.Register(new AssetBrowserTool(
            _gl, Path.Combine(AppContext.BaseDirectory, "Assets")));
#endif
    }

    private void OnFrame(double _)
    {
        if (!_fatalError.HasError)
        {
            _fatalError.TryCatch(() => FrameBody(_), "FrameBody");
        }

        if (_fatalError.HasError)
        {
            _fatalError.Draw();
        }

        try { _imgui.Render(); }
        catch { }
    }

    private void FrameBody(double _)
    {
        _profiler.BeginFrame();
        _frameArena.Reset();
        _resources.ProcessUploads();

        ProcessDebugHotkeys();
        Input.BeginFrame();

        double frameTime = UpdateClock();
        TimeEngine.Update((float)frameTime);
        _imgui.Update(TimeEngine.UnscaledDeltaTime);

        UpdateUIInput();
        UpdateGameLogic();

        RenderScene();
        RenderDebugImGui();


        Input.EndFrame();
        _profiler.EndFrame();
    }

    private void ProcessDebugHotkeys()
    {
#if DEBUG
        _debug.ProcessHotkeys(Input);
#endif
    }

    private double UpdateClock()
    {
        double now = _clock.Elapsed.TotalSeconds;
        double frameTime = now - _lastTime;
        _lastTime = now;
        return frameTime > 0.25 ? 0.25 : frameTime;
    }

    private void UpdateUIInput()
    {
        _ui.Update(
            Input.MousePosition,
            Input.IsMouseDown(Silk.NET.Input.MouseButton.Left),
            Input.WasMousePressed(Silk.NET.Input.MouseButton.Left),
            Width,
            Height);
    }

    private void UpdateGameLogic()
    {
        if (!_debug.BlocksGameInput)
        {
            using (Profiler.Measure("UpdateV"))
                UpdateVariable(TimeEngine.DeltaTime);
        }

        Scheduler.Update(TimeEngine.DeltaTime);
        Coroutines.Update(TimeEngine.DeltaTime);
        Scenes.Update(TimeEngine.DeltaTime);

        if (!_debug.BlocksGameInput)
        {
            _accumulator += TimeEngine.DeltaTime;
            while (_accumulator >= FixedDt)
            {
                using (Profiler.Measure("Update"))
                    Update(FixedDt);
                _accumulator -= FixedDt;
            }
        }
    }

    private void RenderScene()
    {
        _gl.ClearColor(0.08f, 0.09f, 0.11f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        using (Profiler.Measure("Render"))
            Render();
    }

    private void RenderDebugImGui()
    {
        if (!_fatalError.HasError)
            _fatalError.TryCatch(() => OnImGui(), "OnImGui");

        _debug.Draw();

        try { _imgui.Render(); }
        catch { }
    }

    private void OnClosing()
    {
        Unload();

        Safe.Run("Audio.Dispose", () => _audio.Dispose());
        Safe.Run("AudioEngine.Dispose", () => _audioEngine.Dispose());
        Safe.Run("Resources.Dispose", () => _resources.Dispose());

        _debug.Dispose();
        _imgui.Dispose();
        _inputCtx.Dispose();
        _gl.Dispose();
        _jobs.Dispose(); // ОДИН раз, не два
    }

    protected virtual void Load() { }
    protected virtual void Update(double dt)
    {
        
    }
    protected virtual void Render()
    {
        Scenes.Render();
    }
    protected virtual void OnImGui()
    {
        Scenes.OnImGui();
    }
    protected virtual void Unload() { }
    protected virtual void UpdateVariable(float dt)
    {
        Scenes.UpdateVariable(dt);
    }

    public void Dispose() => _window.Dispose();
}