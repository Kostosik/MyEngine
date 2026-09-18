namespace MyEngine.GameFlow;

/// <summary>
/// Базовый класс для игровых состояний. Наследники переопределяют
/// только нужные методы — остальные остаются пустыми.
/// </summary>
public abstract class GameStateBase : IGameState
{
    public virtual void Enter() { }
    public virtual void Update(float dt) { }
    public virtual void Exit() { }

    /// <summary>
    /// Переменный апдейт — вызывается каждый кадр рендера (не 60 Гц).
    /// Используется для отзывчивого ввода и движения.
    /// </summary>
    public virtual void UpdateVariable(float dt) { }
}