using MyEngine;
using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Effects;
using MyEngine.Ecs;
using MyEngine.GameFlow;
using MyEngine.Rendering;
using MyEngine.Systems;
using System.Numerics;

namespace FactoryGame;

/// <summary>
/// Главный класс игры. Наследуется от GameSession — каркас
/// жизненного цикла уже готов. Мы только реализуем конкретные шаги.
/// </summary>
public sealed class FactoryGameApp : GameSession
{
    // === Данные сессии (пересоздаются при рестарте) ===
    private GameState _state = null!;
    private Entity _player = null!;
    private EntityFactoryRegistry _factories = null!;
    public FactoryGameApp() : base("Factory Game", 1280, 720) { }

    // ============================================================
    // 1. Ресурсы — создаются ОДИН раз при старте приложения
    // ============================================================

    protected override void InitializeResources()
    {
        // GPU-ресурсы
        Batch = new SpriteBatch(GL);
        SpriteRenderer = new SpriteRenderSystem(Batch);

        // Шрифт
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "main.ttf");
        if (File.Exists(fontPath))
            Font = new Font(GL, fontPath, 16f);

        // Частицы
        Particles = new ParticleSystem(4096);

        // Реестр фабрик — регистрируется ОДИН раз
        _factories = new EntityFactoryRegistry();
        RegisterFactories();

        // Камера — базовые настройки, конкретное следует за игроком позже
        Camera.DeadzoneSize = new Vector2(100, 80);
        Camera.FollowSmoothing = 6f;
    }

    // ============================================================
    // 2. Старт сессии — при каждом рестарте. Создать мир, state
    // ============================================================

    protected override void StartSession()
    {
        // Мир и состояние
        _state = new GameState();
        World.SetSingleton(_state);
        World.AttachEvents(Events);

        // Контекст — базовые ссылки
        Context.App = this;
        Context.World = World;
        Context.Camera = Camera;
        Context.Events = Events;
        Context.Particles = Particles;
        Context.Font = Font;
    }

    // ============================================================
    // 3. Готовность загрузки
    // ============================================================

    protected override bool IsLoadingComplete()
    {
        // Пока всё синхронно — сразу готово
        return true;
    }

    // ============================================================
    // 4. Загрузка завершена — создать сущности и системы
    // ============================================================

    protected override void CompleteSession()
    {
        _player = _factories.Spawn(World, "player", new Vector2(500, 500));
        // Универсальные системы — из движка
        UpdateSystems.Add(new MovementSystem(Jobs));
        UpdateSystems.Add(new TopDownControllerSystem());

        // Игровая система — из FactoryGame
        VariableSystems.Add(new FactoryGame.Systems.PlayerInputSystem(this, _player));

        var t = _player.Get<Transform>()!;
        Camera.Position = t.Position;

        RegisterConsoleCommands();
    }

    // ============================================================
    // 5. Update / Render
    // ============================================================

    protected override void Update(double dt)
    {
        if (IsLoading) return;

        _state.Update((float)dt);
        UpdateSystems.Update(World, (float)dt);
    }

    protected override void UpdateVariable(float dt)
    {
        // Проверяем загрузку КАЖДЫЙ кадр рендера
        TickLoading();

        if (IsLoading) return;

        VariableSystems.Update(World, dt);

        // Камера за игроком
        var t = _player.Get<Transform>();
        if (t != null)
        {
            Camera.Follow(t.Position, Vector2.Zero, dt);
            Camera.ApplyBounds(Width, Height);
            Camera.UpdateShake(dt);
        }
    }

    protected override void Render()
    {
        GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
        GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        var proj = Camera.GetProjection(Width, Height);
        Batch.Begin(proj);
        SpriteRenderer.Draw(World);
        ParticleRenderer.Draw(Batch, Particles);
        Batch.End();
    }

    protected override void OnImGui()
    {
        // Пока ничего — здесь будут панели UI
    }

    // ============================================================
    // Вспомогательное
    // ============================================================

    private void RegisterFactories()
    {
        _factories.Register("player", (w, pos) => CreatePlayer(w, pos));
    }

    private Entity CreatePlayer(World world, Vector2 position)
    {
        var e = world.Create();
        e.Add(new PlayerTag());
        e.Add(new MyEngine.Components.Transform { Position = position });
        e.Add(new Velocity());
        e.Add(new MyEngine.Components.Collider
        {
            Size = new Vector2(24, 24),
            IsStatic = false,
            Layer = MyEngine.Physics.Layer.Player,
            CollidesWith = MyEngine.Physics.Layer.Wall
        });
        e.Add(new MyEngine.Components.Sprite
        {
            Size = new Vector2(24, 24),
            Color = new Vector4(0.3f, 0.8f, 0.4f, 1f)
        });
        e.Add(new TopDownController { MaxSpeed = 250f });
        return e;
    }

    private bool _consoleRegistered;

    private void RegisterConsoleCommands()
    {
        if (_consoleRegistered) return;
        _consoleRegistered = true;

        Console.Register("spawn", "Spawn entity by type", "<type> <x> <y>",
            args =>
            {
                if (args.Length < 3) return "usage: spawn <type> <x> <y>";
                if (!float.TryParse(args[1], out float x)) return "invalid x";
                if (!float.TryParse(args[2], out float y)) return "invalid y";

                var name = args[0];
                if (!_factories.Has(name))
                    return $"unknown type: '{name}'. known: {string.Join(", ", _factories.Names)}";

                var e = _factories.Spawn(World, name, new Vector2(x, y));
                return $"spawned {name} #{e.Id} at ({x}, {y})";
            });

        Console.Register("factories.list", "List all registered factories", "",
            args =>
            {
                if (_factories.Count == 0) return "no factories registered";
                return $"registered ({_factories.Count}): {string.Join(", ", _factories.Names)}";
            });

        Console.Register("tp", "Teleport player", "<x> <y>",
            args =>
            {
                if (args.Length < 2) return "usage: tp <x> <y>";
                if (!float.TryParse(args[0], out float x)) return "invalid x";
                if (!float.TryParse(args[1], out float y)) return "invalid y";

                var t = _player.Get<Transform>();
                if (t == null) return "no player";
                t.Position = new Vector2(x, y);
                return $"teleported to ({x}, {y})";
            });

        Console.Register("where", "Player position", "",
            args =>
            {
                var t = _player.Get<Transform>();
                return t != null ? $"({t.Position.X:F0}, {t.Position.Y:F0})" : "no player";
            });

        Console.Register("kick", "Restart session", "",
            args => { BeginNewSession(); return "restarting"; });
    }
}