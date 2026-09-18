namespace MyEngine.Ai;

public sealed class Transition<TState> where TState : struct, Enum
{
    public TState From;
    public TState To;
    public Func<bool> Condition;
    public int Priority;
    public bool FromAny;

    public Transition(TState from, TState to, Func<bool> condition, int priority)
    {
        From = from;
        To = to;
        Condition = condition;
        Priority = priority;
    }
}