using ImGuiNET;
using MyEngine.Ai;
using MyEngine.Animation;
using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Dialogue;
using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.Events;
using MyEngine.GameFlow;
using MyEngine.InputEngine;
using MyEngine.Math;
using MyEngine.Rendering;
using MyEngine.Serialization.Binary;
using MyEngine.Systems;
using MyEngine.Time;
using MyEngine.UI;
using Silk.NET.OpenGL;
using System.Numerics;
using TestRPGGame.Components;
using TestRPGGame.DebugGame;
using TestRPGGame.Effects;
using TestRPGGame.Events;
using TestRPGGame.Spawning;
using TestRPGGame.States;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame;

public sealed class MyGame : GameSession
{
    // ============================================================
    // РЕСУРСЫ (создаются один раз)
    // ============================================================
    private SpriteRenderSystem _spriteRenderer = null!;
    private AttackHitboxRenderer _attackHitboxRenderer = null!;
    private DebugDraw _debugDraw = null!;
    private GameDebugOverlay _debugOverlay = null!;
    private RenderTarget _sceneTarget = null!;
    private PostProcessStack _postProcess = null!;
    private LightmapRenderer _lightmapRenderer = null!;
    private RenderTarget _lightmapRT = null!;
    private LightingPass _lightingPass = null!;
    protected override States.GameContext CreateContext()
    => new TestRPGGame.States.GameContext();

    private new States.GameContext Context
         => (States.GameContext)base.Context;

    private bool _statesRegistered;
    private bool _consoleRegistered;
    private bool _watchRegistered;

    // ============================================================
    // СЕССИЯ (пересоздаётся при рестарте)
    // ============================================================
    private GameState _state = null!;
    private Entity _player = null!;
    private NavGrid? _navGrid;
    private LevelUpMenu _menu = null!;
    private GameOverScreen _gameOver = null!;
    private DialogueBox _dialogue = null!;
    private QuestSystem _quest = null!;
    private DamagePopupSystem _popups = null!;
    private WorldInspectorWindow _worldInspectorWindow = null!;
    private EntityInspector _inspector = null!;
    private GameHud _gameHud = null!;
    private DialogueRunner _dialogueRunner = null!;
    private DialogueContext _dialogueContext = null!;
    private readonly Dictionary<string, DialogueTree> _dialogues = new();

    private AsyncMapLoader _mapLoader = null!;
    private string _mapPath = "";

    public MyGame() : base("Lighthouse Keeper", 1280, 720) { }

    // ============================================================
    // 1. РЕСУРСЫ
    // ============================================================

    protected override void InitializeResources()
    {
        Batch = new SpriteBatch(GL);
        _spriteRenderer = new SpriteRenderSystem(Batch);
        _attackHitboxRenderer = new AttackHitboxRenderer(Batch);
        _debugDraw = new DebugDraw(Camera);
        _debugOverlay = new GameDebugOverlay(_debugDraw);

        //var fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "main.ttf");
        Font = new Font(GL, Assets.PathFont("main.ttf"), 20f);

        Particles = new ParticleSystem(capacity: 4096);

        Camera.DeadzoneSize = new Vector2(80, 60);
        Camera.LookAheadDistance = new Vector2(60, 0);
        Camera.LookAheadSmoothing = 4f;
        Camera.FollowSmoothing = 6f;
        Camera.Bounds = new Aabb(new Vector2(0, 0), new Vector2(2000, 2000));

        _sceneTarget = new RenderTarget(GL, Width, Height);
        _lightmapRT = new RenderTarget(GL, Width, Height);

        _postProcess = new PostProcessStack(GL, Width, Height);
        _lightmapRenderer = new LightmapRenderer(GL);
        _lightingPass = new LightingPass(GL);

        _postProcess.Add(_lightingPass);
        _postProcess.Add(new BloomPass(GL, threshold: 0.7f, blurRadius: 1.5f, strength: 1.8f));
        _postProcess.Add(PostProcessPresets.Vignette(GL, intensity: 0.7f));

        UI.Batch = Batch;
        UI.Font = Font;
        UI.White = Texture2D.White(GL);
        UI.Rounded = UIRounded;

        BinaryComponentRegistry _binaryRegistry = new BinaryComponentRegistry();
        _binaryRegistry.Register(() => new Transform());
        _binaryRegistry.Register(() => new Velocity());
        _binaryRegistry.Register(() => new Collider());
        _binaryRegistry.Register(() => new Health());

        BinaryWorldSerializer _binarySerializer = new BinaryWorldSerializer(_binaryRegistry);

        RegisterConsoleCommands();
        RegisterWatchValues();
    }

    private void RegisterStates()
    {
        if (_statesRegistered) return;
        _statesRegistered = true;
        // "Loading" больше не нужен — GameSession сам ждёт загрузку.
        // Регистрируем только игровые фазы.
        StateMachine.Register("Playing", new PlayingState(Context));
        StateMachine.Register("Dialogue", new DialogueState(Context));
        StateMachine.Register("Dead", new DeadState(Context));
        StateMachine.Register("Victory", new VictoryState(Context));
    }

    // ============================================================
    // 2. СТАРТ СЕССИИ
    // ============================================================

    protected override void StartSession()
    {
        // Связать scheduler и профайлер — один раз
        Context.UpdateSystems = UpdateSystems;
        Context.VariableSystems = VariableSystems;
        Context.Profiler = Profiler;
        Context.Restart = BeginNewSession;
        Context.FinishLoading = CompleteSession;

        // Мир
        World.SetSingleton(new GameState());
        World.AttachEvents(Events);
        _state = World.GetSingleton<GameState>()!;
        _state.IsLoading = true;

        // Игровые сервисы
        _popups = new DamagePopupSystem();
        _dialogue = new DialogueBox();
        _worldInspectorWindow = new WorldInspectorWindow(World);
        Context.WorldInspector = _worldInspectorWindow;

        // Обновить контекст
        Context.State = _state;
        Context.Player = null!;
        Context.Popups = _popups;
        Context.Dialogue = _dialogue;
        Context.Menu = null!;
        Context.GameOver = null!;

        // Состояния — пересоздаются с новым Context
        RegisterStates();

        // Загрузка
        _mapPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Maps", "island.json");
        _mapLoader = new AsyncMapLoader(Jobs);
        _mapLoader.Start(_mapPath);
    }

    // ============================================================
    // 3. ГОТОВНОСТЬ
    // ============================================================

    protected override bool IsLoadingComplete() => _mapLoader.IsDone;

    // ============================================================
    // 4. ЗАГРУЗКА ЗАВЕРШЕНА
    // ============================================================

    protected override void CompleteSession()
    {
        // Карта
        _navGrid = _mapLoader.ApplyTo(World);

        // Диалоги
        _dialogueContext = new DialogueContext();
        _dialogueRunner = new DialogueRunner(Events);
        _dialogues.Clear();
        BuildDialogues();

        // Статистика
        _state.EnemiesTotal = World.Query().With<EnemyTag>().Count();
        _state.SparksTotal = World.Query().With<Pickup>().Count();

        // Игрок
        _player = EntityFactory.CreatePlayer(World, new Vector2(300, 300));
        SetupPlayerAnimation();

        // Системы
        RegisterGameSystems();

        // Камера
        Camera.Position = _player.Get<Transform>()!.Position;

        // Свет
        CreateLights();

        // UI
        _menu = new LevelUpMenu(_state, _player);
        _gameOver = new GameOverScreen(_state);

        if (UI.Screens.Count == 0)
        {
            _gameHud = new GameHud(_state, _player);
            UI.Add(_gameHud);
        }

        // Инспектор — пересоздаём на новый мир
        _inspector = new EntityInspector(World, Camera, () => Width, () => Height);

        // Контекст
        Context.Player = _player;
        Context.Menu = _menu;
        Context.GameOver = _gameOver;

        // Подписки на события
        RegisterEventSubscriptions();

        // Загрузка завершена — переходим в Playing
        _state.IsLoading = false;
        StateMachine.SetInitial("Playing");

        // Логи
        Log.Info("World", WorldInspector.Report(World));
        GameDiagnostics.LogInteractables(World);
        GameDiagnostics.LogPickups(World);
    }

    // ============================================================
    // Вспомогательные
    // ============================================================

    private void SetupPlayerAnimation()
    {
        var tex = new Texture2D(GL, Path.Combine(
            AppContext.BaseDirectory, "Assets", "Textures", "player_walk.png"));
        var frames = SpriteSheet.Slice(tex, 32, 32);

        var walkAnim = new AnimationEngine
        {
            Name = "walk",
            Texture = tex,
            Frames = frames,
            Fps = 8f,
            Loop = true
        };

        _player.Add(new AnimatorComponent());
        _player.Get<AnimatorComponent>()!.Play(walkAnim);
    }

    private void RegisterGameSystems()
    {
        _quest = new QuestSystem(_state, _player, _popups);
        _attackHitboxRenderer.SetPlayer(_player);

        Context.UpdateSystems.Add(new PlayerFlickerSystem());
        Context.UpdateSystems.Add(new HitFlashSystem());
        Context.UpdateSystems.Add(new AISystem());
        Context.UpdateSystems.Add(new AnimationSystem());
        Context.UpdateSystems.Add(new MovementSystem(Jobs));
        Context.UpdateSystems.Add(new CollisionSystem());
        Context.UpdateSystems.Add(new CombatSystem(this, _player, _state, _popups, Events));
        Context.UpdateSystems.Add(new ProgressionSystem(_player, _state));
        Context.UpdateSystems.Add(new PickupSystem(_player, _state, _popups));
        Context.UpdateSystems.Add(new TriggerSystem(Events));
        Context.UpdateSystems.Add(new LightPulseSystem());

        Context.VariableSystems.Add(new PlayerInputSystem(this, _player, _state));
        Context.VariableSystems.Add(new TopDownControllerSystem());
        Context.VariableSystems.Add(new ParallaxSystem(Camera));
    }

    private void CreateLights()
    {
        var ambient = World.Create();
        ambient.Add(new AmbientLight { Color = new Vector3(0.2f, 0.2f, 0.35f) });

        var torch = World.Create();
        torch.Add(new Transform { Position = new Vector2(1600, 180) });
        torch.Add(new PointLight
        {
            Color = new Vector3(1f, 0.8f, 0.4f),
            Radius = 300f,
            Intensity = 1.5f,
            Falloff = 2f,
            Pulsate = true,
            PulsateSpeed = 4f,
            PulsateAmount = 0.1f
        });

        var playerLight = World.Create();
        playerLight.Add(new Transform { Position = Vector2.Zero });
        playerLight.Add(new PointLight
        {
            Color = new Vector3(0.9f, 0.9f, 1f),
            Radius = 150f,
            Intensity = 0.6f,
            Falloff = 2f
        });
        VariableSystems.Add(new LightFollowSystem(_player, playerLight));
    }

    private void RegisterEventSubscriptions()
    {
        Events.Subscribe<EnemyKilledEvent>(evt =>
        {
            if (evt.XpReward > 0)
                _state.AddXp(evt.XpReward);
            Particles.Emit(ParticlePresets.DeathPuff, evt.Position);
        });

        Events.Subscribe<PlayerDamagedEvent>(evt =>
        {
            Particles.Emit(ParticlePresets.PlayerHurt, evt.Position);
            Camera.Shake(20f, 0.25f);
        });

        Events.Subscribe<EnemyDamagedEvent>(evt =>
            Particles.Emit(ParticlePresets.HitSpark, evt.Position));

        Events.Subscribe<InteractionRequestedEvent>(OnInteractionRequested);
    }

    private void OnInteractionRequested(InteractionRequestedEvent evt)
    {
        var interactable = evt.Target.Get<Interactable>();
        if (interactable == null) return;

        if (interactable.Id == "lighthouse" && _state.CanLightLighthouse())
        {
            _state.Phase = GamePhase.Victory;
            return;
        }

        if (_dialogues.TryGetValue(interactable.Id, out var tree))
        {
            _dialogueRunner.Start(tree, _dialogueContext);
            _dialogue.Open(_dialogueRunner);
            return;
        }

        var fallback = DialogueTree.FromLines(
            interactable.Id, interactable.Speaker, $"({interactable.Prompt})");
        _dialogueRunner.Start(fallback, _dialogueContext);
        _dialogue.Open(_dialogueRunner);
    }

    private void BuildDialogues()
    {
        var keeper = new DialogueTree
        {
            Id = "keeper",
            Speaker = "Keeper",
            StartNode = "intro"
        };

        keeper.Nodes["intro"] = new DialogueNode
        {
            Id = "intro",
            Text = "The lighthouse went dark three nights ago.",
            NextNode = "intro2"
        };
        keeper.Nodes["intro2"] = new DialogueNode
        {
            Id = "intro2",
            Text = "Three sparks fell from its lamp. Will you find them?",
            Choices = new List<DialogueChoice>
            {
                new() { Text = "Yes, I'll help.", NextNode = "yes", SetFlag = "keeper_quest_started" },
                new() { Text = "What's in it for me?", NextNode = "greedy" },
                new() { Text = "No.", NextNode = "no" }
            }
        };
        keeper.Nodes["yes"] = new DialogueNode
        {
            Id = "yes",
            Text = "Thank you. Search the island. I will wait here.",
            NextNode = ""
        };
        keeper.Nodes["greedy"] = new DialogueNode
        {
            Id = "greedy",
            Text = "The light of the old keepers will bless you.",
            NextNode = "yes"
        };
        keeper.Nodes["no"] = new DialogueNode
        {
            Id = "no",
            Text = "Then we are lost to the dark.",
            NextNode = ""
        };

        _dialogues["keeper"] = keeper;
        _dialogues["lighthouse"] = DialogueTree.FromLines(
            "lighthouse", "Lighthouse",
            "The lamp is dark. It hungers for the sparks.");
    }

    // ============================================================
    // КОНСОЛЬ — регистрируется ОДИН раз
    // ============================================================

    private void RegisterConsoleCommands()
    {
        if (_consoleRegistered) return;
        _consoleRegistered = true;

        Console.Register("god", "Toggle invulnerability", "",
            args =>
            {
                var hp = Context.Player?.Get<Health>();
                if (hp == null) return "no player";
                hp.Invulnerable = !hp.Invulnerable;
                return $"God mode: {hp.Invulnerable}";
            });

        Console.Register("hp", "Set player HP", "<amount>",
            args =>
            {
                if (args.Length < 1) return "usage: hp <amount>";
                if (!int.TryParse(args[0], out int value)) return "invalid";
                var hp = Context.Player?.Get<Health>();
                if (hp == null) return "no player";
                hp.Hp = value;
                return $"HP = {value}";
            });

        Console.Register("xp", "Add XP", "<amount>",
            args =>
            {
                if (args.Length < 1) return "usage: xp <amount>";
                if (!int.TryParse(args[0], out int value)) return "invalid";
                Context.State?.AddXp(value);
                return $"+{value} XP";
            });

        Console.Register("info", "Show entity by id", "<id>",
            args =>
            {
                if (args.Length < 1) return "usage: info <id>";
                if (!int.TryParse(args[0], out int id)) return "invalid";
                var e = Context.World?.GetById(id);
                if (e == null) return $"entity {id} not found";
                return $"#{id}: {string.Join(", ", e.ComponentTypes.Select(t => t.Name))}";
            });

        Console.Register("tp", "Teleport player", "<x> <y>",
            args =>
            {
                if (args.Length < 2) return "usage: tp <x> <y>";
                if (!float.TryParse(args[0], out float x)) return "invalid x";
                if (!float.TryParse(args[1], out float y)) return "invalid y";
                var t = Context.Player?.Get<Transform>();
                if (t == null) return "no player";
                t.Position = new Vector2(x, y);
                return $"teleported to ({x}, {y})";
            });

        Console.Register("kick", "Restart session", "",
            args => { BeginNewSession(); return "restarting"; });

        Console.Register("where", "Player position", "",
            args =>
            {
                var t = Context.Player?.Get<Transform>();
                return t != null ? $"({t.Position.X:F0}, {t.Position.Y:F0})" : "no player";
            });
    }

    // ============================================================
    // WATCH — регистрируется ОДИН раз
    // ============================================================

    private void RegisterWatchValues()
    {
        if (_watchRegistered) return;
        _watchRegistered = true;

        Watch.AddSection("Player");
        Watch.Add("HP", () => Context.Player?.Get<Health>()?.Hp.ToString() ?? "-");
        Watch.Add("Position", () =>
        {
            var t = Context.Player?.Get<Transform>();
            return t != null ? $"({t.Position.X:F0}, {t.Position.Y:F0})" : "-";
        });
        Watch.Add("Level", () => Context.State?.PlayerLevel.ToString() ?? "-");
        Watch.Add("XP", () => Context.State != null
            ? $"{Context.State.PlayerXp} / {Context.State.XpToNext}" : "-");

        Watch.AddSection("World");
        Watch.Add("Entities", () =>
            Context.World?.Entities.Count(e => e.IsAlive).ToString() ?? "-");
        Watch.Add("Enemies", () =>
            Context.World?.Query().With<EnemyTag>().Count().ToString() ?? "-");
        Watch.Add("Pickups", () =>
            Context.World?.Query().With<PickupTag>().Count().ToString() ?? "-");

        Watch.AddSection("Game");
        Watch.Add("Phase", () => Context.State?.Phase.ToString() ?? "-");
        Watch.Add("IsLoading", () => IsLoading.ToString());

        Watch.AddSection("Time");
        Watch.Add("TimeScale", () => TimeEngine.TimeScale.ToString("F2"));
        Watch.Add("Session", () => TimeEngine.FormatSession());
        Watch.Add("Frame", () => TimeEngine.Frame.ToString());
    }

    // ============================================================
    // UPDATE / RENDER
    // ============================================================

    protected override void UpdateVariable(float dt)
    {
        Assert.True(dt >= 0, $"Negative dt: {dt}");

        // Проверка загрузки — каждый кадр рендера
        TickLoading();
        if (IsLoading) return;

        _gameHud?.Refresh();

        // Инспектор (debug) — одна проверка, не три
#if DEBUG
        if (DebugConfig.Available)
        {
            // F6 — World Inspector (окно со списком сущностей по компонентам)
            if (Input.ConsumeDebugPressed(DebugAction.ToggleWorldInspector))
                _worldInspectorWindow.Visible = !_worldInspectorWindow.Visible;

            // F7 — NavGrid paths (линии путей AI)
            if (Input.ConsumeDebugPressed(DebugAction.ToggleNavGridPaths))
                NavGridDebugDraw.ShowPaths = !NavGridDebugDraw.ShowPaths;

            // F8 — NavGrid grid (сетка клеток)
            if (Input.ConsumeDebugPressed(DebugAction.ToggleNavGridGrid))
                NavGridDebugDraw.ShowGrid = !NavGridDebugDraw.ShowGrid;

            // F10 — Entity Inspector (окно инспектора сущностей)
            if (Input.ConsumeDebugPressed(DebugAction.ToggleEntityInspector))
                _inspector.Visible = !_inspector.Visible;

            if (!UI.IsMouseOver(Input.MousePosition)
    && Input.ConsumeMousePressed(Silk.NET.Input.MouseButton.Right))
            {
                _inspector.PickAt(Input.MousePosition);
            }
        }
#endif

        StateMachine.UpdateVariable(dt);
    }

    protected override void Update(double dt)
    {


        if (IsLoading) return;

        Assert.True(dt >= 0, $"Negative dt: {dt}");
        _state.Tick((float)dt);
        StateMachine.Update((float)dt);
    }

    protected override void Render()
    {
        if (IsLoading) return;

        // 1. Сцена
        _sceneTarget.Bind();
        GL.ClearColor(0.08f, 0.09f, 0.11f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var proj = Camera.GetProjection(Width, Height);
        Batch.Begin(proj);
        _spriteRenderer.Draw(World);
        _attackHitboxRenderer.Draw();
        ParticleRenderer.Draw(Batch, Particles);
        Batch.End();

        _sceneTarget.Unbind(Width, Height);

        // 2. Lightmap
        _lightmapRT.Bind();
        _lightmapRenderer.Render(World, Camera, Width, Height);
        _lightmapRT.Unbind(Width, Height);

        _lightingPass.SetLightmap(_lightmapRT.GetColorTextureAsTexture2D());

        // 3. Постобработка
        var sceneTex = _sceneTarget.GetColorTextureAsTexture2D();
        var finalTex = _postProcess.Process(sceneTex, Width, Height);

        // 4. Вывод
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var screenProj = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1f, 1f);
        Batch.Begin(screenProj);
        Batch.Draw(finalTex,
            new Vector2(Width / 2f, Height / 2f),
            new Vector2(Width, Height),
            uvMin: new Vector2(0f, 1f),
            uvMax: new Vector2(1f, 0f),
            0f, new Vector2(0.5f), Vector4.One);
        Batch.End();

        // 5. UI
        // Проход 1 — фон (скруглённые прямоугольники)
        UIRounded.Begin(screenProj);
        UI.DrawBackground();
        UIRounded.End();

        // Проход 2 — текст и картинки
        Batch.Begin(screenProj);
        UI.DrawForeground();
        Batch.End();

        GL.Disable(EnableCap.Blend);
    }

    protected override void OnImGui()
    {
        // Игровые панели (ImGui рисуется поверх)
        if (_state.IsLoading)
        {
            DrawLoadingScreen();
            return;
        }

        _popups.Draw(Camera);

        if (_state.Phase == GamePhase.Playing)
        {
            DrawInteractPrompt();
            _menu.Draw();
            _dialogue.Draw();
        }
        else
        {
            _gameOver.Draw();
        }

#if DEBUG
        // === Отладка ===
        if (DebugConfig.ShowGizmos)
            _debugOverlay.Draw(World);
            NavGridDebugDraw.Draw(World, _navGrid, Camera, Width, Height);

        if (DebugConfig.ShowProfiler)
            ProfilerWindow.Draw();

        if (DebugConfig.ShowTimeline)
            TimelineWindow.Draw();

        if (_worldInspectorWindow.Visible)
            _worldInspectorWindow.Draw();

        if (_inspector.Visible)
            _inspector.Draw();
#endif
    }

    private void DrawLoadingScreen()
    {
        var io = ImGui.GetIO();
        var bg = ImGui.GetBackgroundDrawList();
        bg.AddRectFilled(Vector2.Zero, io.DisplaySize,
            ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)));

        var center = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f);
        string text = "Loading...";
        var size = ImGui.CalcTextSize(text);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2(center.X - size.X * 0.5f, center.Y - size.Y * 0.5f),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.85f, 0.85f, 0.9f, 1f)),
            text);
    }

    private void DrawInteractPrompt()
    {
        if (_dialogue.IsOpen) return;

        var interactor = _player.Get<Interactor>();
        if (interactor?.CurrentTarget == null) return;

        var i = interactor.CurrentTarget.Get<Interactable>();
        var t = interactor.CurrentTarget.Get<Transform>();
        if (i == null || t == null) return;

        var screen = Camera.WorldToScreen(t.Position + new Vector2(0, -50), Width, Height);
        var dl = ImGui.GetForegroundDrawList();
        string text = $"[E] {i.Prompt}";
        var size = ImGui.CalcTextSize(text);
        dl.AddText(
            new Vector2(screen.X - size.X * 0.5f, screen.Y - size.Y * 0.5f),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)),
            text);
    }

    protected override void Unload()
    {
        Batch.Dispose();
    }
}