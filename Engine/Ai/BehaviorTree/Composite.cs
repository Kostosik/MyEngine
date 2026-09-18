
namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Базовая нода с детьми. Sequence, Selector, Parallel наследуют её.
/// </summary>
public abstract class Composite : Node
{
    protected readonly List<Node> Children = new();

    protected Composite(params Node[] children)
    {
        Children.AddRange(children);
    }

    public void Add(Node node) => Children.Add(node);

    public override void ResetTree()
    {
        Reset();
        foreach (var c in Children) c.ResetTree();
    }
}