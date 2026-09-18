using MyEngine.Ecs;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Корень дерева поведения. Тикается раз в кадр.
/// Содержит Blackboard — общую память нод.
/// </summary>
public sealed class BehaviorTreeEngine
{
    public Node Root { get; }
    public Blackboard Blackboard { get; } = new();

    private NodeStatus _lastStatus = NodeStatus.Success;

    public BehaviorTreeEngine(Node root) => Root = root;

    public NodeStatus LastStatus => _lastStatus;

    public NodeStatus Tick(Entity self, World world, float dt)
    {
        var ctx = new TickContext(self, world, dt, Blackboard);
        _lastStatus = Root.Tick(ctx);

        // После завершения — сброс всего дерева, чтобы следующий кадр начал заново
        if (_lastStatus != NodeStatus.Running)
            Root.ResetTree();

        return _lastStatus;
    }
}