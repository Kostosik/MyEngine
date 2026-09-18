using MyEngine.Ecs;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Всё, что нода получает при Tick.
/// Передаётся по стеку — не аллоцируется, если не менять.
/// </summary>
public readonly struct TickContext
{
    public readonly Entity Self;
    public readonly World World;
    public readonly float Dt;
    public readonly Blackboard Blackboard;

    public TickContext(Entity self, World world, float dt, Blackboard blackboard)
    {
        Self = self;
        World = world;
        Dt = dt;
        Blackboard = blackboard;
    }
}