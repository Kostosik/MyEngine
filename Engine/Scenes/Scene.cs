using MyEngine.Ecs;
using MyEngine.GameFlow;
using MyEngine.Rendering;
using MyEngine.Systems;

namespace MyEngine.Scenes;

/// <summary>
/// Базовый класс сцены. Одна сцена = один игровой мир со своей логикой.
///
/// Жизненный цикл:
///   OnLoad       — при первом появлении в стеке.
///   Update       — фиксированный шаг (60 Гц).
///   UpdateVariable — каждый кадр рендера.
///   Render       — рисует мир.
///   OnImGui      — ImGui поверх.
///   OnUnload     — при выгрузке из стека.
/// </summary>
public abstract class Scene
{
    public string Name { get; set; } = "";

    /// <summary>Мир этой сцены. Пересоздаётся в OnLoad.</summary>
    public World World { get; protected set; } = new();

    /// <summary>Игровой контекст. Создаётся в OnLoad.</summary>
    public GameContext Context { get; protected set; } = null!;

    /// <summary>Системы фиксированного шага.</summary>
    public SystemScheduler UpdateSystems { get; } = new();

    /// <summary>Системы переменного шага.</summary>
    public SystemScheduler VariableSystems { get; } = new();

    /// <summary>Идёт ли загрузка. Пока true — Update/UpdateVariable не тикают.</summary>
    public bool IsLoading { get; protected set; }

    // === Стек ===

    /// <summary>Если true и сцена НЕ верхняя — она всё равно рисуется.</summary>
    public bool RenderBelow { get; set; } = false;

    /// <summary>Если true и сцена НЕ верхняя — её Update не вызывается.</summary>
    public bool PauseBelow { get; set; } = true;

    // ============================================================
    // Жизненный цикл
    // ============================================================

    public virtual void OnLoad(Application app) { }
    public virtual void OnUnload(Application app) { }

    public virtual void Update(Application app, float dt) { }
    public virtual void UpdateVariable(Application app, float dt) { }

    public virtual void Render(Application app) { }
    public virtual void OnImGui(Application app) { }

    // ============================================================
    // Вспомогательное
    // ============================================================

    /// <summary>Пересоздать мир. Полезно для рестарта уровня.</summary>
    protected void ResetWorld()
    {
        World = new World();
    }

    /// <summary>
    /// Проверка готовности загрузки. Переопределяется в игре.
    /// Вызывать каждый кадр рендера — если вернёт true, IsLoading
    /// станет false, и сцена начнёт тикать.
    /// </summary>
    protected virtual bool IsLoadingComplete() => true;

    /// <summary>Обработать завершение загрузки. Переопределяется в игре.</summary>
    protected virtual void OnLoadingComplete() { }

    /// <summary>
    /// Вызывать каждый кадр рендера сцены, пока IsLoading == true.
    /// Когда готово — вызывает OnLoadingComplete один раз.
    /// </summary>
    public void TickLoading()
    {
        if (!IsLoading) return;
        if (IsLoadingComplete())
        {
            IsLoading = false;
            OnLoadingComplete();
        }
    }

    /// <summary>Установить флаг загрузки вручную. Например, при старте сессии.</summary>
    protected void BeginLoading() => IsLoading = true;
}