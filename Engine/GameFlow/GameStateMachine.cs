namespace MyEngine.GameFlow;

public sealed class GameStateMachine
{
    private readonly Dictionary<string, GameStateBase> _states = new();
    private GameStateBase? _current;
    private string _currentName = "";
    private string? _pendingTransition;

    public string CurrentName => _currentName;

    public void Register(string name, GameStateBase state)
    {
        _states[name] = state;
    }

    public void RequestTransition(string name)
    {
        if (!_states.ContainsKey(name))
            throw new InvalidOperationException($"State not registered: {name}");
        _pendingTransition = name;
    }

    public void SetInitial(string name)
    {
        if (!_states.TryGetValue(name, out var s))
            throw new InvalidOperationException($"State not registered: {name}");
        _current = s;
        _currentName = name;
        _current.Enter();
    }

    public void Update(float dt)
    {
        if (_pendingTransition != null)
        {
            _current?.Exit();
            _currentName = _pendingTransition;
            _current = _states[_currentName];
            _pendingTransition = null;
            _current.Enter();
        }

        _current?.Update(dt);
    }

    public void UpdateVariable(float dt)
    {
        _current?.UpdateVariable(dt);
    }

    public GameStateBase? Current => _current;
}