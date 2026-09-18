using MyEngine.Ecs;

namespace MyEngine.Scenes;

/// <summary>
/// Базовый класс сцены. Одна сцена = один игровой мир со своей логикой.
///
/// Каждая сцена владеет своим World. При переключении старый World
/// уничтожается, новый создаётся в OnLoad.
///
/// Жизненный цикл:
///   OnLoad       — при первом появлении в стеке. Создай мир, спавни сущности.
///   Update       — каждый фиксированный шаг (60 Гц).
///   UpdateVariable — каждый кадр рендера (переменный dt).
///   Render       — рисует мир через свой SpriteBatch.
///   OnImGui      — ImGui поверх (HUD, отладочные панели).
///   OnUnload     — при выгрузке из стека. Освободи ресурсы.
/// </summary>
public abstract class Scene
{
    /// <summary>Имя сцены (для отладки и переходов).</summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Мир этой сцены. Создаётся в конструкторе, может быть
    /// пересоздан в OnLoad (например, при рестарте уровня).
    /// </summary>
    public World World { get; protected set; } = new();

    /// <summary>
    /// Если true и сцена НЕ верхняя в стеке — она всё равно рисуется.
    /// Используется для паузы: игра на фоне, меню поверх.
    /// </summary>
    public bool RenderBelow { get; set; } = false;

    /// <summary>
    /// Если true и сцена НЕ верхняя — её Update не вызывается.
    /// Обычно true (пауза), но можно поставить false для фоновых эффектов.
    /// </summary>
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

    /// <summary>
    /// Пересоздать мир с нуля. Полезно для рестарта уровня.
    /// </summary>
    protected void ResetWorld()
    {
        World = new World();
    }
}