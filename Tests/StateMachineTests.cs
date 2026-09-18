using MyEngine.Ai;

namespace MyEngine.Tests;

public class StateMachineTests
{
    private enum TestState { A, B, C }

    [Fact]
    public void StartsInInitialState()
    {
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.Start();
        Assert.Equal(TestState.A, fsm.Current);
    }

    [Fact]
    public void Transition_WhenConditionTrue()
    {
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.AddState(TestState.B);
        bool go = false;
        fsm.AddTransition(TestState.A, TestState.B, () => go);
        fsm.Start();

        fsm.Tick(0.1f);
        Assert.Equal(TestState.A, fsm.Current);

        go = true;
        fsm.Tick(0.1f);
        Assert.Equal(TestState.B, fsm.Current);
    }

    [Fact]
    public void OnEnter_And_OnExit_Called()
    {
        var log = new List<string>();
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A).OnEnter(() => log.Add("A.enter")).OnExit(() => log.Add("A.exit"));
        fsm.AddState(TestState.B).OnEnter(() => log.Add("B.enter"));
        fsm.AddTransition(TestState.A, TestState.B, () => true);
        fsm.Start();

        fsm.Tick(0.1f);

        Assert.Equal(new[] { "A.enter", "A.exit", "B.enter" }, log);
    }

    [Fact]
    public void OnTick_CalledEachFrame()
    {
        int ticks = 0;
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A).OnTick(_ => ticks++);
        fsm.Start();

        fsm.Tick(0.1f);
        fsm.Tick(0.1f);
        fsm.Tick(0.1f);

        Assert.Equal(3, ticks);
    }

    [Fact]
    public void PriorityWins()
    {
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.AddState(TestState.B);
        fsm.AddState(TestState.C);

        // Оба условия истинны, но приоритет B выше
        fsm.AddTransition(TestState.A, TestState.C, () => true, priority: 1);
        fsm.AddTransition(TestState.A, TestState.B, () => true, priority: 5);
        fsm.Start();

        fsm.Tick(0.1f);
        Assert.Equal(TestState.B, fsm.Current);
    }

    [Fact]
    public void AnyTransition_FiresFromAnyState()
    {
        var dead = false;
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.AddState(TestState.B);
        fsm.AddState(TestState.C);

        fsm.AddAnyTransition(TestState.C, () => dead, priority: 100);
        fsm.AddTransition(TestState.A, TestState.B, () => true);
        fsm.Start();

        fsm.Tick(0.1f);
        Assert.Equal(TestState.B, fsm.Current);

        dead = true;
        fsm.Tick(0.1f);
        Assert.Equal(TestState.C, fsm.Current);
    }

    [Fact]
    public void TimeInState_Resets()
    {
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.AddState(TestState.B);
        fsm.AddTransition(TestState.A, TestState.B, () => true);
        fsm.Start();

        fsm.Tick(0.5f);
        Assert.Equal(TestState.B, fsm.Current);
        Assert.Equal(0f, fsm.TimeInState, 3);
    }

    [Fact]
    public void TimeInState_GrowsOverTicks()
    {
        var fsm = new StateMachine<TestState>(TestState.A);
        fsm.AddState(TestState.A);
        fsm.Start();

        fsm.Tick(0.5f);
        fsm.Tick(0.3f);
        fsm.Tick(0.2f);

        Assert.Equal(1.0f, fsm.TimeInState, 3);
        Assert.Equal(TestState.A, fsm.Current);
    }
}