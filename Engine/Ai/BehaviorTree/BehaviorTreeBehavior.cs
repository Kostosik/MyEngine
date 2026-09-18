using MyEngine.Ai;
using MyEngine.Ecs;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Обёртка над BehaviorTree, реализующая IBehavior.
/// Позволяет использовать BT как замену FSM в одной игре.
/// </summary>
public sealed class BehaviorTreeBehavior : IBehavior
{
    private readonly BehaviorTreeEngine _tree;

    public BehaviorTreeBehavior(BehaviorTreeEngine tree) => _tree = tree;

    public BehaviorTreeEngine Tree => _tree;

    public void Tick(Entity self, World world, float dt)
    {
        _tree.Tick(self, world, dt);
    }
}