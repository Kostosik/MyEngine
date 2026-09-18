using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Повторяет ребёнка N раз (-1 — бесконечно) и возвращает Success.
/// Если ребёнок Failure — Repeater тоже Failure.
/// </summary>
public sealed class Repeater : Decorator
{
    private readonly int _count;
    private int _done;

    public Repeater(Node child, int count = -1) : base(child)
    {
        _count = count;
    }

    public override NodeStatus Tick(TickContext ctx)
    {
        if (_count > 0 && _done >= _count)
        {
            Reset();
            return NodeStatus.Success;
        }

        var s = Child.Tick(ctx);

        if (s == NodeStatus.Running) return NodeStatus.Running;
        if (s == NodeStatus.Failure) { Reset(); return NodeStatus.Failure; }

        _done++;
        Child.Reset();

        if (_count > 0 && _done >= _count)
        {
            Reset();
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    public override void Reset() => _done = 0;
}