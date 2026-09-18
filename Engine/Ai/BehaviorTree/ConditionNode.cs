using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Условие — лямбда, возвращающая bool.
/// true → Success, false → Failure. Running не бывает.
/// </summary>
public sealed class ConditionNode : Node
{
    private readonly Func<TickContext, bool> _condition;

    public ConditionNode(Func<TickContext, bool> condition) => _condition = condition;

    public override NodeStatus Tick(TickContext ctx)
        => _condition(ctx) ? NodeStatus.Success : NodeStatus.Failure;
}