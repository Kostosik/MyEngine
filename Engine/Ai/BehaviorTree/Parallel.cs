using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

public enum ParallelPolicy
{
    /// <summary>Успех если все дети Success. Failure при первом Failure.</summary>
    RequireAll,

    /// <summary>Успех при первом Success. Failure если все Failure.</summary>
    RequireOne
}

/// <summary>
/// Parallel: все дети тикаются каждый кадр.
/// Возвращает результат по выбранной политике.
/// </summary>
public sealed class Parallel : Composite
{
    private readonly ParallelPolicy _policy;
    private NodeStatus[] _statuses = System.Array.Empty<NodeStatus>();

    public Parallel(ParallelPolicy policy, params Node[] children) : base(children)
    {
        _policy = policy;
    }

    public override NodeStatus Tick(TickContext ctx)
    {
        if (_statuses.Length != Children.Count)
            _statuses = new NodeStatus[Children.Count];

        int running = 0;
        int success = 0;
        int failure = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            // Уже завершённые дети не тикают повторно
            if (_statuses[i] != NodeStatus.Running)
            {
                _statuses[i] = Children[i].Tick(ctx);
            }

            switch (_statuses[i])
            {
                case NodeStatus.Running: running++; break;
                case NodeStatus.Success: success++; break;
                case NodeStatus.Failure: failure++; break;
            }
        }

        if (_policy == ParallelPolicy.RequireAll)
        {
            if (failure > 0) { Reset(); return NodeStatus.Failure; }
            if (success == Children.Count) { Reset(); return NodeStatus.Success; }
            return NodeStatus.Running;
        }
        else // RequireOne
        {
            if (success > 0) { Reset(); return NodeStatus.Success; }
            if (failure == Children.Count) { Reset(); return NodeStatus.Failure; }
            return NodeStatus.Running;
        }
    }

    public override void Reset()
    {
        for (int i = 0; i < _statuses.Length; i++) _statuses[i] = NodeStatus.Running;
    }
}