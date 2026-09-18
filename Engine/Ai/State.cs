namespace MyEngine.Ai;

public sealed class State<TState> where TState : struct, Enum
{
    public TState Id { get; }

    private Action? _onEnter;
    private Action? _onExit;
    private Action<float>? _onTick;

    internal State(TState id) => Id = id;

    public State<TState> OnEnter(Action a) { _onEnter = a; return this; }
    public State<TState> OnExit(Action a) { _onExit = a; return this; }
    public State<TState> OnTick(Action<float> a) { _onTick = a; return this; }

    internal void InvokeEnter() => _onEnter?.Invoke();
    internal void InvokeExit() => _onExit?.Invoke();
    internal void InvokeTick(float dt) => _onTick?.Invoke(dt);
}