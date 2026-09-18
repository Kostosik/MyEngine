namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Базовая нода дерева поведения.
/// Каждый Tick возвращает Success/Failure/Running.
///
/// Reset вызывается когда дерево начинает новый "цикл"
/// (например, после завершения корня) — ноды со состоянием
/// должны сбросить свои счётчики/таймеры.
/// </summary>
public abstract class Node
{
    public abstract NodeStatus Tick(TickContext ctx);
    public virtual void Reset() { }

    /// <summary>Рекурсивный сброс всего поддерева.</summary>
    public virtual void ResetTree() => Reset();
}