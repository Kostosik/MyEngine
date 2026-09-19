namespace MyEngine.Ai.BehaviorTree;

/// <summary>Инвертирует результат ребёнка. Success ↔ Failure. Running не меняется.</summary>
public sealed class Inverter : Decorator
{
    public Inverter(Node child) : base(child) { }

    public override NodeStatus Tick(TickContext ctx)
    {
        var s = Child.Tick(ctx);
        return s switch
        {
            NodeStatus.Success => NodeStatus.Failure,
            NodeStatus.Failure => NodeStatus.Success,
            _ => NodeStatus.Running
        };
    }
}