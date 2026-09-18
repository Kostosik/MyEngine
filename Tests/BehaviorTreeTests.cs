using MyEngine.Ai.BehaviorTree;
using MyEngine.BehaviorTree;
using MyEngine.Ecs;

namespace MyEngine.Tests;

public class BehaviorTreeTests
{
    private static TickContext Ctx(float dt = 0.1f)
    {
        var world = new World();
        var entity = world.Create();
        return new TickContext(entity, world, dt, new Blackboard());
    }

    private static Node Success() => new ActionNode(_ => NodeStatus.Success);
    private static Node Failure() => new ActionNode(_ => NodeStatus.Failure);
    private static Node Running() => new ActionNode(_ => NodeStatus.Running);

    [Fact]
    public void Sequence_AllSuccess_ReturnsSuccess()
    {
        var seq = new Sequence(Success(), Success(), Success());
        Assert.Equal(NodeStatus.Success, seq.Tick(Ctx()));
    }

    [Fact]
    public void Sequence_AnyFailure_ReturnsFailure()
    {
        var seq = new Sequence(Success(), Failure(), Success());
        Assert.Equal(NodeStatus.Failure, seq.Tick(Ctx()));
    }

    [Fact]
    public void Sequence_RunningPropagates()
    {
        var seq = new Sequence(Success(), Running(), Success());
        Assert.Equal(NodeStatus.Running, seq.Tick(Ctx()));
    }

    [Fact]
    public void Selector_FirstSuccessWins()
    {
        int secondCalled = 0;
        var sel = new Selector(
            Failure(),
            new ActionNode(_ => { secondCalled++; return NodeStatus.Success; }),
            new ActionNode(_ => { secondCalled++; return NodeStatus.Success; }));

        Assert.Equal(NodeStatus.Success, sel.Tick(Ctx()));
        Assert.Equal(1, secondCalled);
    }

    [Fact]
    public void Selector_AllFail_ReturnsFailure()
    {
        var sel = new Selector(Failure(), Failure());
        Assert.Equal(NodeStatus.Failure, sel.Tick(Ctx()));
    }

    [Fact]
    public void Inverter_FlipsResult()
    {
        Assert.Equal(NodeStatus.Failure, new Inverter(Success()).Tick(Ctx()));
        Assert.Equal(NodeStatus.Success, new Inverter(Failure()).Tick(Ctx()));
    }

    [Fact]
    public void Succeeder_AlwaysSuccess()
    {
        Assert.Equal(NodeStatus.Success, new Succeeder(Failure()).Tick(Ctx()));
        Assert.Equal(NodeStatus.Success, new Succeeder(Success()).Tick(Ctx()));
    }

    [Fact]
    public void WaitNode_ReturnsRunningUntilElapsed()
    {
        var wait = new WaitNode(0.5f);

        Assert.Equal(NodeStatus.Running, wait.Tick(Ctx(0.2f)));
        Assert.Equal(NodeStatus.Running, wait.Tick(Ctx(0.2f)));
        Assert.Equal(NodeStatus.Success, wait.Tick(Ctx(0.2f)));
    }

    [Fact]
    public void Blackboard_StoreAndRetrieve()
    {
        var bb = new Blackboard();
        bb.Set("target", 42);
        Assert.Equal(42, bb.Get<int>("target"));

        bb.SetFloat("hp", 3.5f);
        Assert.Equal(3.5f, bb.GetFloat("hp"), 2);
    }
}