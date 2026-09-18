using MyEngine.Ai.BehaviorTree;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>Базовая нода, оборачивающая одного ребёнка.</summary>
public abstract class Decorator : Node
{
    protected readonly Node Child;

    protected Decorator(Node child) => Child = child;

    public override void ResetTree()
    {
        Reset();
        Child.ResetTree();
    }
}