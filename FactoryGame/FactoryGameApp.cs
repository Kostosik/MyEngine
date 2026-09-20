//using FactoryGame.DebugGame;
//using MyEngine;
//using MyEngine.Ai;
//using MyEngine.Assets;
//using MyEngine.Components;
//using MyEngine.Diagnostics;
//using MyEngine.Dialogue;
//using MyEngine.Ecs;
//using MyEngine.Effects;
//using MyEngine.GameFlow;
//using MyEngine.InputEngine;
//using MyEngine.Math;
//using MyEngine.Rendering;
//using MyEngine.Serialization.Binary;
//using MyEngine.Systems;
//using MyEngine.UI;
//using System.Numerics;

//namespace FactoryGame;

///// <summary>
///// Главный класс игры. Наследуется от GameSession — каркас
///// жизненного цикла уже готов. Мы только реализуем конкретные шаги.
///// </summary>
//public sealed class FactoryGameApp : //GameSession
//{
//    // === Данные сессии (пересоздаются при рестарте) ===
//    private GameState _state = null!;
//    private Entity _player = null!;
//    private EntityFactoryRegistry _factories = null!;

//    private WorldInspectorWindow _worldInspectorWindow = null!;
//    private EntityInspector _inspector = null!;

//    BinaryComponentRegistry _binaryRegistry = null!;
//    BinaryWorldSerializer _binarySerializer = null!;

//    public FactoryGameApp() : base("Factory Game", 1280, 720) { }

//    // ============================================================
//    // 1. Ресурсы — создаются ОДИН раз при старте приложения
//    // ============================================================

//    private SpriteRenderSystem _spriteRenderer = null!;
//    private DebugDraw _debugDraw = null!;
//    private GameDebugOverlay _debugOverlay = null!;
//    private RenderTarget _sceneTarget = null!;
//    private PostProcessStack _postProcess = null!;
//    private LightmapRenderer _lightmapRenderer = null!;
//    private RenderTarget _lightmapRT = null!;
//    private LightingPass _lightingPass = null!;

//    protected override States.GameContext CreateContext()
//=> new FactoryGame.States.GameContext();
//    private new States.GameContext Context
//     => (States.GameContext)base.Context;

//    private bool _statesRegistered;
//    private bool _consoleRegistered;
//    private bool _watchRegistered;

//    protected override void InitializeResources()
//    {
//        // GPU-ресурсы
//        Batch = new SpriteBatch(GL);
//        SpriteRenderer = new SpriteRenderSystem(Batch);

//        // Шрифт
//        Font = new Font(GL, Assets.PathFont("main.ttf"), 16f);
//        // Частицы
//        Particles = new ParticleSystem(4096);

//        // Реестр фабрик — регистрируется ОДИН раз
//        _factories = new EntityFactoryRegistry();
//        RegisterFactories();

//        // Камера — базовые настройки, конкретное следует за игроком позже
//        Camera.DeadzoneSize = new Vector2(80, 60);
//        Camera.LookAheadDistance = new Vector2(60, 0);
//        Camera.LookAheadSmoothing = 4f;
//        Camera.FollowSmoothing = 6f;
//        Camera.Bounds = new Aabb(new Vector2(0, 0), new Vector2(2000, 2000));

//        _sceneTarget = new RenderTarget(GL, Width, Height);
//        _lightmapRT = new RenderTarget(GL, Width, Height);

//        _postProcess = new PostProcessStack(GL, Width, Height);
//        _lightmapRenderer = new LightmapRenderer(GL);
//        _lightingPass = new LightingPass(GL);

//        _postProcess.Add(_lightingPass);
//        _postProcess.Add(new BloomPass(GL, threshold: 0.7f, blurRadius: 1.5f, strength: 1.8f));
//        _postProcess.Add(PostProcessPresets.Vignette(GL, intensity: 0.7f));

//        UI.Batch = Batch;
//        UI.Font = Font;
//        UI.White = Texture2D.White(GL);
//        UI.Rounded = UIRounded;

//        _binaryRegistry = new BinaryComponentRegistry();
//        _binaryRegistry.Register(() => new Transform());
//        _binaryRegistry.Register(() => new Velocity());
//        _binaryRegistry.Register(() => new Collider());
//        _binaryRegistry.Register(() => new Health());

//        _binarySerializer = new BinaryWorldSerializer(_binaryRegistry);

//        RegisterConsoleCommands();
//        RegisterWatchValues();
//    }



//    // ============================================================
//    // 2. Старт сессии — при каждом рестарте. Создать мир, state
//    // ============================================================

//    protected override void StartSession()
//    {
//        // Связать scheduler и профайлер — один раз
//        Context.UpdateSystems = UpdateSystems;
//        Context.VariableSystems = VariableSystems;
//        Context.Profiler = Profiler;
//        Context.Restart = BeginNewSession;
//        Context.FinishLoading = CompleteSession;

//        // Мир и состояние
//        _state = new GameState();
//        World.SetSingleton(_state);
//        World.AttachEvents(Events);

//        // Контекст — базовые ссылки
//        Context.App = this;
//        Context.World = World;
//        Context.Camera = Camera;
//        Context.Events = Events;
//        Context.Particles = Particles;
//        Context.Font = Font;

//        RegisterStates();
//    }

//    private void RegisterStates()
//    {
//        if (_statesRegistered) return;
//        _statesRegistered = true;
//        // Регистрируем только игровые фазы.
//        //StateMachine.Register("Playing", new PlayingState(Context));
//        //StateMachine.Register("Dialogue", new DialogueState(Context));
//        //StateMachine.Register("Dead", new DeadState(Context));
//        //StateMachine.Register("Victory", new VictoryState(Context));
//    }

//    // ============================================================
//    // 3. Готовность загрузки
//    // ============================================================

//    protected override bool IsLoadingComplete()
//    {
//        // Пока всё синхронно — сразу готово
//        return true;
//    }

//    // ============================================================
//    // 4. Загрузка завершена — создать сущности и системы
//    // ============================================================

//    protected override void CompleteSession()
//    {
//        _player = _factories.Spawn(World, "player", new Vector2(500, 500));
//        // Универсальные системы — из движка


//        var t = _player.Get<Transform>()!;
//        Camera.Position = t.Position;

//        RegisterGameSystems();

//        // Инспектор — пересоздаём на новый мир
//        _inspector = new EntityInspector(World, Camera, () => Width, () => Height);
//        Context.Player = _player;

//        RegisterEventSubscriptions();

//        _state.IsLoading = false;

//        Log.Info("World", WorldInspector.Report(World));
//    }

//    // ============================================================
//    // 5. Update / Render
//    // ============================================================

//    protected override void Update(double dt)
//    {
//        if (IsLoading) return;

//        _state.Update((float)dt);
//        UpdateSystems.Update(World, (float)dt);
//    }

//    protected override void UpdateVariable(float dt)
//    {
//        // Проверяем загрузку КАЖДЫЙ кадр рендера
//        TickLoading();

//        if (IsLoading) return;

//        VariableSystems.Update(World, dt);

//        // Камера за игроком
//        var t = _player.Get<Transform>();
//        if (t != null)
//        {
//            Camera.Follow(t.Position, Vector2.Zero, dt);
//            Camera.ApplyBounds(Width, Height);
//            Camera.UpdateShake(dt);
//        }

//#if DEBUG
//        if (DebugConfig.Available)
//        {
//            // F10 — Entity Inspector (окно инспектора сущностей)
//            if (Input.ConsumeDebugPressed(DebugAction.ToggleEntityInspector))
//                _inspector.Visible = !_inspector.Visible;

//            if (!UI.IsMouseOver(Input.MousePosition)
//    && Input.ConsumeMousePressed(Silk.NET.Input.MouseButton.Right))
//            {
//                _inspector.PickAt(Input.MousePosition);
//            }
//        }
//#endif
//    }

//    protected override void Render()
//    {
//        GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
//        GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

//        var proj = Camera.GetProjection(Width, Height);
//        Batch.Begin(proj);
//        SpriteRenderer.Draw(World);
//        ParticleRenderer.Draw(Batch, Particles);
//        Batch.End();
//    }

//    protected override void OnImGui()
//    {
//        // Пока ничего — здесь будут панели UI
//    }

//    // ============================================================
//    // Вспомогательное
//    // ============================================================

//    private void RegisterFactories()
//    {
//        _factories.Register("player", (w, pos) => CreatePlayer(w, pos));
//    }

//    private void RegisterGameSystems()
//    {
//        Context.VariableSystems.Add(new MovementSystem(Jobs));
//        Context.VariableSystems.Add(new TopDownControllerSystem());

//        // Игровая система — из FactoryGame
//        Context.VariableSystems.Add(new FactoryGame.Systems.PlayerInputSystem(this, _player));
//    }

//    private void RegisterEventSubscriptions()
//    {

//    }

//    private Entity CreatePlayer(World world, Vector2 position)
//    {
//        var e = world.Create();
//        e.Add(new PlayerTag());
//        e.Add(new Transform { Position = position });
//        e.Add(new Velocity());
//        e.Add(new Collider
//        {
//            Size = new Vector2(24, 24),
//            IsStatic = false,
//            Layer = MyEngine.Physics.Layer.Player,
//            CollidesWith = MyEngine.Physics.Layer.Wall
//        });
//        e.Add(new Sprite
//        {
//            Size = new Vector2(24, 24),
//            Color = new Vector4(0.3f, 0.8f, 0.4f, 1f)
//        });
//        e.Add(new TopDownController { MaxSpeed = 250f });
//        return e;
//    }

//    private void RegisterWatchValues()
//    {

//    }

//    private void RegisterConsoleCommands()
//    {
//        if (_consoleRegistered) return;
//        _consoleRegistered = true;

//        Console.Register("spawn", "Spawn entity by type", "<type> <x> <y>",
//            args =>
//            {
//                if (args.Length < 3) return "usage: spawn <type> <x> <y>";
//                if (!float.TryParse(args[1], out float x)) return "invalid x";
//                if (!float.TryParse(args[2], out float y)) return "invalid y";

//                var name = args[0];
//                if (!_factories.Has(name))
//                    return $"unknown type: '{name}'. known: {string.Join(", ", _factories.Names)}";

//                var e = _factories.Spawn(World, name, new Vector2(x, y));
//                return $"spawned {name} #{e.Id} at ({x}, {y})";
//            });

//        Console.Register("factories.list", "List all registered factories", "",
//            args =>
//            {
//                if (_factories.Count == 0) return "no factories registered";
//                return $"registered ({_factories.Count}): {string.Join(", ", _factories.Names)}";
//            });

//        Console.Register("tp", "Teleport player", "<x> <y>",
//            args =>
//            {
//                if (args.Length < 2) return "usage: tp <x> <y>";
//                if (!float.TryParse(args[0], out float x)) return "invalid x";
//                if (!float.TryParse(args[1], out float y)) return "invalid y";

//                var t = _player.Get<Transform>();
//                if (t == null) return "no player";
//                t.Position = new Vector2(x, y);
//                return $"teleported to ({x}, {y})";
//            });

//        Console.Register("where", "Player position", "",
//            args =>
//            {
//                var t = _player.Get<Transform>();
//                return t != null ? $"({t.Position.X:F0}, {t.Position.Y:F0})" : "no player";
//            });

//        Console.Register("kick", "Restart session", "",
//            args => { BeginNewSession(); return "restarting"; });
//    }
//}