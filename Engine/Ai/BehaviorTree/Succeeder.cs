namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Всегда возвращает Success (если ребёнок не Running).
/// Используется как "неважно, что вышло — продолжаем".
/// </summary>
public sealed class Succeeder : Decorator
{
    public Succeeder(Node child) : base(child) { }

    public override NodeStatus Tick(TickContext ctx)
    {
        var s = Child.Tick(ctx);
        if (s == NodeStatus.Running) return NodeStatus.Running;
        Child.Reset();
        return NodeStatus.Success;
    }
}