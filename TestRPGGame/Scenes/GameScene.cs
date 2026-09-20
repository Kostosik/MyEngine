using ImGuiNET;
using MyEngine;
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
using MyEngine.Scenes;
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

namespace TestRPGGame.Scenes;

public sealed class GameScene : Scene
{
    // === Ресурсы рендера (пересоздаются при входе в сцену) ===
    private SpriteRenderSystem _spriteRenderer = null!;
    private AttackHitboxRenderer _attackHitboxRenderer = null!;
    private DebugDraw _debugDraw = null!;
    private GameDebugOverlay _debugOverlay = null!;
    private RenderTarget _sceneTarget = null!;
    private PostProcessStack _postProcess = null!;
    private LightmapRenderer _lightmapRenderer = null!;
    private RenderTarget _lightmapRT = null!;
    private LightingPass _lightingPass = null!;

    // === Игровое состояние ===
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

    private TestRPGGame.States.GameContext Context = null!;


    private AsyncMapLoader _mapLoader = null!;
    private string _mapPath = "";

    private bool _consoleRegistered;
    private bool _watchRegistered;
    private bool _statesRegistered;

    public GameScene()
    {
        Name = "Game";
    }

    // ============================================================
    // Жизненный цикл
    // ============================================================

    public override void OnLoad(Application app)
    {
        // === Ресурсы рендера ===
        _spriteRenderer = new SpriteRenderSystem(app.Batch);
        _attackHitboxRenderer = new AttackHitboxRenderer(app.Batch);
        _debugDraw = new DebugDraw(app.Camera);
        _debugOverlay = new GameDebugOverlay(_debugDraw);

        _sceneTarget = new RenderTarget(app.GL, app.Width, app.Height);
        _lightmapRT = new RenderTarget(app.GL, app.Width, app.Height);

        _postProcess = new PostProcessStack(app.GL, app.Width, app.Height);
        _lightmapRenderer = new LightmapRenderer(app.GL);
        _lightingPass = new LightingPass(app.GL);

        _postProcess.Add(_lightingPass);
        _postProcess.Add(new BloomPass(app.GL, threshold: 0.7f, blurRadius: 1.5f, strength: 1.8f));
        _postProcess.Add(PostProcessPresets.Vignette(app.GL, intensity: 0.7f));

        // === Мир и контекст ===
        ResetWorld();
        Context = new TestRPGGame.States.GameContext
        {
            App = app,
            World = World,
            Camera = app.Camera,
            Events = app.Events,
            Particles = app.Particles,
            Font = app.Font,
            UpdateSystems = UpdateSystems,
            VariableSystems = VariableSystems,
            Profiler = app.Profiler,
            Restart = () => app.Scenes.Load("Game"),
            FinishLoading = OnLoadingComplete
        };

        // === Игровое состояние ===
        World.SetSingleton(new GameState());
        World.AttachEvents(app.Events);
        _state = World.GetSingleton<GameState>()!;

        _popups = new DamagePopupSystem();
        _dialogue = new DialogueBox();
        _worldInspectorWindow = new WorldInspectorWindow(World);
        Context.WorldInspector = _worldInspectorWindow;

        Context.State = _state;
        Context.Player = null!;
        Context.Popups = _popups;
        Context.Dialogue = _dialogue;
        Context.Menu = null!;
        Context.GameOver = null!;

        // === Состояния игры ===
        RegisterStates(app);

        // === Загрузка карты ===
        _mapPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Maps", "island.json");
        _mapLoader = new AsyncMapLoader(app.Jobs);
        _mapLoader.Start(_mapPath);

        BeginLoading();
    }

    public override void OnUnload(Application app)
    {
        // Освобождение GPU-ресурсов
        _sceneTarget?.Dispose();
        _lightmapRT?.Dispose();
        _lightmapRenderer?.Dispose();
        _lightingPass?.Dispose();
        _postProcess?.Dispose();

        // Очистка Watch/Console — при следующем входе зарегистрируются заново
        Watch.Clear();
        // Console команды оставляем — они перезапишутся (Dictionary)

        // Состояния удаляем из StateMachine
        // (если у тебя StateMachine не умеет Clear — пропусти, Register перезапишет)
    }

    protected override bool IsLoadingComplete() => _mapLoader.IsDone;

    protected override void OnLoadingComplete()
    {
        // Карта
        _navGrid = _mapLoader.ApplyTo(World);

        // Диалоги
        _dialogueContext = new DialogueContext();
        _dialogueRunner = new DialogueRunner(Context.Events);
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
        Context.Camera.Position = _player.Get<Transform>()!.Position;

        // Свет
        CreateLights();

        // UI
        _menu = new LevelUpMenu(_state, _player);
        _gameOver = new GameOverScreen(_state);

        _gameHud = new GameHud(_state, _player);
        Context.App.UI.Add(_gameHud);

        _inspector = new EntityInspector(World, Context.Camera, () => Context.App.Width, () => Context.App.Height);

        Context.Player = _player;
        Context.Menu = _menu;
        Context.GameOver = _gameOver;

        // Подписки
        RegisterEventSubscriptions();
        RegisterConsoleCommands(Context.App);
        RegisterWatchValues();

        // Финал
        Context.App.AppGameStateMachine.SetInitial("Playing");

        Log.Info("World", WorldInspector.Report(World));
        GameDiagnostics.LogInteractables(World);
        GameDiagnostics.LogPickups(World);
    }

    // ============================================================
    // Update / Render
    // ============================================================

    public override void Update(Application app, float dt)
    {
        if (IsLoading) return;
        _state.Tick(dt);
        app.AppGameStateMachine.Update(dt);
    }

    public override void UpdateVariable(Application app, float dt)
    {
        TickLoading();
        if (IsLoading) return;

        _gameHud?.Refresh();

        // Инспектор (debug)
#if DEBUG
        if (DebugConfig.Available)
        {
            if (app.Input.ConsumeDebugPressed(DebugAction.ToggleWorldInspector))
                _worldInspectorWindow.Visible = !_worldInspectorWindow.Visible;
            if (app.Input.ConsumeDebugPressed(DebugAction.ToggleNavGridPaths))
                NavGridDebugDraw.ShowPaths = !NavGridDebugDraw.ShowPaths;
            if (app.Input.ConsumeDebugPressed(DebugAction.ToggleNavGridGrid))
                NavGridDebugDraw.ShowGrid = !NavGridDebugDraw.ShowGrid;
            if (app.Input.ConsumeDebugPressed(DebugAction.ToggleEntityInspector))
                _inspector.Visible = !_inspector.Visible;

            if (!app.UI.IsMouseOver(app.Input.MousePosition)
                && app.Input.ConsumeMousePressed(Silk.NET.Input.MouseButton.Right))
            {
                _inspector.PickAt(app.Input.MousePosition);
            }
        }
#endif

        app.AppGameStateMachine.UpdateVariable(dt);
    }

    public override void Render(Application app)
    {
        if (IsLoading)
        {
            // Пустой экран — Application всё равно UI нарисует
            app.GL.ClearColor(0, 0, 0, 1);
            app.GL.Clear(ClearBufferMask.ColorBufferBit);
            return;
        }

        var GL = app.GL;
        var Camera = Context.Camera;
        var Batch = app.Batch;

        // 1. Сцена
        _sceneTarget.Bind();
        GL.ClearColor(0.08f, 0.09f, 0.11f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var proj = Camera.GetProjection(app.Width, app.Height);
        Batch.Begin(proj);
        _spriteRenderer.Draw(World);
        _attackHitboxRenderer.Draw();
        ParticleRenderer.Draw(Batch, app.Particles);
        Batch.End();

        _sceneTarget.Unbind(app.Width, app.Height);

        // 2. Lightmap
        _lightmapRT.Bind();
        _lightmapRenderer.Render(World, Camera, app.Width, app.Height);
        _lightmapRT.Unbind(app.Width, app.Height);

        _lightingPass.SetLightmap(_lightmapRT.GetColorTextureAsTexture2D());

        // 3. Постобработка
        var sceneTex = _sceneTarget.GetColorTextureAsTexture2D();
        var finalTex = _postProcess.Process(sceneTex, app.Width, app.Height);

        // 4. Вывод
        GL.ClearColor(0, 0, 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var screenProj = Matrix4x4.CreateOrthographicOffCenter(0, app.Width, app.Height, 0, -1f, 1f);
        Batch.Begin(screenProj);
        Batch.Draw(finalTex,
            new Vector2(app.Width / 2f, app.Height / 2f),
            new Vector2(app.Width, app.Height),
            uvMin: new Vector2(0f, 1f),
            uvMax: new Vector2(1f, 0f),
            0f, new Vector2(0.5f), Vector4.One);
        Batch.End();

        // 5. UI
        app.UIRounded.Begin(screenProj);
        app.UI.DrawBackground();
        app.UIRounded.End();

        Batch.Begin(screenProj);
        app.UI.DrawForeground();
        Batch.End();

        GL.Disable(EnableCap.Blend);
    }

    public override void OnImGui(Application app)
    {
        if (IsLoading)
        {
            DrawLoadingScreen();
            return;
        }

        _popups.Draw(Context.Camera);

        if (_state.Phase == GamePhase.Playing)
        {
            DrawInteractPrompt(app);
            _menu.Draw();
            _dialogue.Draw();
        }
        else
        {
            _gameOver.Draw();
        }

#if DEBUG
        if (DebugConfig.ShowGizmos)
        {
            _debugOverlay.Draw(World);
            NavGridDebugDraw.Draw(World, _navGrid, Context.Camera, app.Width, app.Height);
        }

        if (DebugConfig.ShowProfiler)
            app.ProfilerWindow.Draw();

        if (DebugConfig.ShowTimeline)
            app.TimelineWindow.Draw();

        if (_worldInspectorWindow.Visible)
            _worldInspectorWindow.Draw();

        if (_inspector.Visible)
            _inspector.Draw();
#endif
    }

    // ============================================================
    // Всё, что было в MyGame — переносим сюда
    // ============================================================

    private void RegisterStates(Application app)
    {
        if (_statesRegistered) return;
        _statesRegistered = true;

        app.AppGameStateMachine.Register("Playing", new PlayingState(Context));
        app.AppGameStateMachine.Register("Dialogue", new DialogueState(Context));
        app.AppGameStateMachine.Register("Dead", new DeadState(Context));
        app.AppGameStateMachine.Register("Victory", new VictoryState(Context));
    }

    private void SetupPlayerAnimation()
    {
        var app = Context.App;
        var tex = new Texture2D(app.GL, Path.Combine(
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
        var app = Context.App;
        _quest = new QuestSystem(_state, _player, _popups);
        _attackHitboxRenderer.SetPlayer(_player);

        UpdateSystems.Add(new PlayerFlickerSystem());
        UpdateSystems.Add(new HitFlashSystem());
        UpdateSystems.Add(new AISystem());
        UpdateSystems.Add(new AnimationSystem());
        UpdateSystems.Add(new MovementSystem(app.Jobs));
        UpdateSystems.Add(new CollisionSystem());
        UpdateSystems.Add(new CombatSystem(app, _player, _state, _popups, app.Events));
        UpdateSystems.Add(new ProgressionSystem(_player, _state));
        UpdateSystems.Add(new PickupSystem(_player, _state, _popups));
        UpdateSystems.Add(new TriggerSystem(app.Events));
        UpdateSystems.Add(new LightPulseSystem());

        VariableSystems.Add(new PlayerInputSystem(app, _player, _state));
        VariableSystems.Add(new TopDownControllerSystem());
        VariableSystems.Add(new ParallaxSystem(Context.Camera));
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
        var app = Context.App;
        app.Events.Subscribe<EnemyKilledEvent>(evt =>
        {
            if (evt.XpReward > 0)
                _state.AddXp(evt.XpReward);
            app.Particles.Emit(ParticlePresets.DeathPuff, evt.Position);
        });

        app.Events.Subscribe<PlayerDamagedEvent>(evt =>
        {
            app.Particles.Emit(ParticlePresets.PlayerHurt, evt.Position);
            Context.Camera.Shake(20f, 0.25f);
        });

        app.Events.Subscribe<EnemyDamagedEvent>(evt =>
            app.Particles.Emit(ParticlePresets.HitSpark, evt.Position));

        app.Events.Subscribe<InteractionRequestedEvent>(OnInteractionRequested);
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
        keeper.Nodes["yes"] = new DialogueNode { Id = "yes", Text = "Thank you.", NextNode = "" };
        keeper.Nodes["greedy"] = new DialogueNode { Id = "greedy", Text = "The light will bless you.", NextNode = "yes" };
        keeper.Nodes["no"] = new DialogueNode { Id = "no", Text = "Then we are lost.", NextNode = "" };

        _dialogues["keeper"] = keeper;
        _dialogues["lighthouse"] = DialogueTree.FromLines(
            "lighthouse", "Lighthouse",
            "The lamp is dark. It hungers for the sparks.");
    }

    private void RegisterConsoleCommands(Application app)
    {
        if (_consoleRegistered) return;
        _consoleRegistered = true;

        var console = app.Console;
        if (console == null) return;

        console.Register("god", "Toggle invulnerability", "", args =>
        {
            var hp = Context.Player?.Get<Health>();
            if (hp == null) return "no player";
            hp.Invulnerable = !hp.Invulnerable;
            return $"God mode: {hp.Invulnerable}";
        });

        console.Register("hp", "Set HP", "<amount>", args =>
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int v)) return "usage: hp <amount>";
            var hp = Context.Player?.Get<Health>();
            if (hp == null) return "no player";
            hp.Hp = v;
            return $"HP = {v}";
        });

        console.Register("xp", "Add XP", "<amount>", args =>
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int v)) return "usage: xp <amount>";
            Context.State?.AddXp(v);
            return $"+{v} XP";
        });

        console.Register("tp", "Teleport", "<x> <y>", args =>
        {
            if (args.Length < 2) return "usage: tp <x> <y>";
            if (!float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y))
                return "invalid";
            var t = Context.Player?.Get<Transform>();
            if (t == null) return "no player";
            t.Position = new Vector2(x, y);
            return $"tp ({x}, {y})";
        });

        console.Register("kick", "Restart session", "", args =>
        {
            app.Scenes.Load("Game");
            return "restarting";
        });
    }

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

        Watch.AddSection("Game");
        Watch.Add("Phase", () => Context.State?.Phase.ToString() ?? "-");
        Watch.Add("IsLoading", () => IsLoading.ToString());
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

    private void DrawInteractPrompt(Application app)
    {
        if (_dialogue.IsOpen) return;
        var interactor = _player.Get<Interactor>();
        if (interactor?.CurrentTarget == null) return;

        var i = interactor.CurrentTarget.Get<Interactable>();
        var t = interactor.CurrentTarget.Get<Transform>();
        if (i == null || t == null) return;

        var screen = Context.Camera.WorldToScreen(t.Position + new Vector2(0, -50), app.Width, app.Height);
        var dl = ImGui.GetForegroundDrawList();
        string text = $"[E] {i.Prompt}";
        var size = ImGui.CalcTextSize(text);
        dl.AddText(
            new Vector2(screen.X - size.X * 0.5f, screen.Y - size.Y * 0.5f),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)),
            text);
    }
}