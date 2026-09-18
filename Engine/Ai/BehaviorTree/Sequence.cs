namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Sequence: дети выполняются по порядку.
/// - Все Success → Sequence Success.
/// - Любой Failure → Sequence Failure.
/// - Ребёнок Running → Sequence Running (на следующем Tick продолжим с него).
///
/// Классика: "проверь врага → подойди → ударь".
/// </summary>
public sealed class Sequence : Composite
{
    private int _current;

    public Sequence(params Node[] children) : base(children) { }

    public override NodeStatus Tick(TickContext ctx)
    {
        while (_current < Children.Count)
        {
            var status = Children[_current].Tick(ctx);

            if (status == NodeStatus.Running) return NodeStatus.Running;
            if (status == NodeStatus.Failure)
            {
                Reset();
                return NodeStatus.Failure;
            }

            _current++;
        }

        Reset();
        return NodeStatus.Success;
    }

    public override void Reset() => _current = 0;
}