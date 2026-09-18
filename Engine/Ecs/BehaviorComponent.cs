using MyEngine.Ai;

namespace MyEngine.Ecs;

public sealed class BehaviorComponent
{
    public IBehavior Behavior = null!;
    public bool Started;

    public BehaviorComponent(IBehavior behavior)
    {
        Behavior = behavior;
    }
}