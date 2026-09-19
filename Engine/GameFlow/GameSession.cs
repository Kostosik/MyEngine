using MyEngine.Coroutines;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.Events;
using MyEngine.Rendering;
using MyEngine.Systems;
using MyEngine.Timers;

namespace MyEngine.GameFlow;

/// <summary>
/// Игровая сессия. Расширяет Application концепцией «сессия» —
/// логический запуск игры, который можно перезапускать.
///
/// Жизненный цикл:
///   1. InitializeResources() — один раз за всё приложение.
///   2. StartSession() — при каждом старте новой сессии.
///   3. TickLoading() — пока IsLoading, проверяет готовность.
///   4. CompleteSession() — когда загрузка завершена.
///   5. BeginNewSession() — перезапуск (из игры).
/// </summary>
public abstract class GameSession : Application
{
    // === Ресурсы (создаются один раз) ===
    public SpriteBatch Batch { get; set; } = null!;
    public SpriteRenderSystem SpriteRenderer { get; set; } = null!;
    protected Font Font { get; set; } = null!;
    public ParticleSystem Particles { get; set; } = null!;
    public Camera2D Camera { get; } = new();

    // === Сессия (пересоздаётся при рестарте) ===
    public World World { get; set; } = null!;
    public GameContext Context { get; set; } = null!;
    public SystemScheduler UpdateSystems { get; } = new();
    public SystemScheduler VariableSystems { get; } = new();
    public bool IsLoading { get; set; }

    /// <summary>
    /// Конструктор. Пробрасывает параметры в Application.
    /// Наследники вызывают base(...) со своими значениями.
    /// </summary>
    protected GameSession(string title = "My Game", int width = 1280, int height = 720)
        : base(title, width, height)
    {
    }

    /// <summary>
    /// Application вызовет Load() — мы подхватываем и запускаем
    /// жизненный цикл сессии.
    /// </summary>
    protected override void Load()
    {
        Initialize();
    }
    // === Инициализация ===

    public void Initialize()
    {
        InitializeResources();
        BeginNewSession();
    }

    public void BeginNewSession()
    {
        Scheduler.Clear();
        Coroutines.StopAll();

        World = new World();
        Context = CreateContext();   // ← игра создаёт свой тип

        // Заполнить базовые поля
        Context.App = this;
        Context.World = World;
        Context.Camera = Camera;
        Camera.ScreenWidth = Width;
        Camera.ScreenHeight = Height;
        Context.Events = Events;
        Context.Particles = Particles;
        Context.Font = Font;

        UpdateSystems.Clear();
        VariableSystems.Clear();
        IsLoading = true;

        StartSession();
    }
    protected virtual GameContext CreateContext() => new GameContext();
    /// <summary>Вызывать каждый Update, пока IsLoading.</summary>
    public void TickLoading()
    {
        if (!IsLoading) return;

        if (IsLoadingComplete())
        {
            IsLoading = false;
            CompleteSession();
        }
    }

    // === Абстрактные методы — их переопределяет игра ===

    /// <summary>Один раз за приложение. Создать GPU-ресурсы, UI, состояния.</summary>
    protected abstract void InitializeResources();

    /// <summary>Старт новой сессии. Создать World, GameState, запустить загрузку.</summary>
    protected abstract void StartSession();

    /// <summary>Готова ли загрузка. Например, mapLoader.IsDone.</summary>
    protected abstract bool IsLoadingComplete();

    /// <summary>Загрузка завершена. Создать игровые сущности и системы.</summary>
    protected abstract void CompleteSession();
}