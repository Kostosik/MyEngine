namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Selector: пробует детей по очереди, пока один не вернёт Success.
/// - Любой Success → Selector Success.
/// - Все Failure → Selector Failure.
/// - Ребёнок Running → Selector Running.
///
/// Классика: "если вижу врага — атакуй. Иначе если голоден — поешь. Иначе патрулируй."
/// </summary>
public sealed class Selector : Composite
{
    private int _current;

    public Selector(params Node[] children) : base(children) { }

    public override NodeStatus Tick(TickContext ctx)
    {
        while (_current < Children.Count)
        {
            var status = Children[_current].Tick(ctx);

            if (status == NodeStatus.Running) return NodeStatus.Running;
            if (status == NodeStatus.Success)
            {
                Reset();
                return NodeStatus.Success;
            }

            _current++;
        }

        Reset();
        return NodeStatus.Failure;
    }

    public override void Reset() => _current = 0;
}