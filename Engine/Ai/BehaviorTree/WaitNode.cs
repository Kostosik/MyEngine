using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Ждёт указанное число секунд. Первый Tick запускает таймер,
/// затем возвращается Running, пока не пройдёт duration.
/// </summary>
public sealed class WaitNode : Node
{
    private readonly float _duration;
    private float _elapsed;
    private bool _started;

    public WaitNode(float duration) => _duration = duration;

    public override NodeStatus Tick(TickContext ctx)
    {
        if (!_started)
        {
            _started = true;
            _elapsed = 0f;
        }

        _elapsed += ctx.Dt;

        if (_elapsed >= _duration)
        {
            Reset();
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        _elapsed = 0f;
        _started = false;
    }
}