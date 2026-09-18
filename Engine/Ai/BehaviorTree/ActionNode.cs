using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Простое действие — лямбда. Возвращает Success/Failure/Running.
/// Если лямбда возвращает void — оборачиваем как Success.
/// </summary>
public sealed class ActionNode : Node
{
    private readonly Func<TickContext, NodeStatus> _action;

    public ActionNode(Func<TickContext, NodeStatus> action) => _action = action;

    public ActionNode(Action<TickContext> action)
        => _action = ctx => { action(ctx); return NodeStatus.Success; };

    public override NodeStatus Tick(TickContext ctx) => _action(ctx);
}