using Silk.NET.Input;

namespace MyEngine.InputEngine;

public sealed class Bindings
{
    private readonly Dictionary<GameAction, Key> _game = new();
    private readonly Dictionary<DebugAction, Key> _debug = new();

    public Bindings()
    {
        // === Игровые ===
        Set(GameAction.MoveUp, Key.W);
        Set(GameAction.MoveDown, Key.S);
        Set(GameAction.MoveLeft, Key.A);
        Set(GameAction.MoveRight, Key.D);
        Set(GameAction.Attack, Key.Space);
        Set(GameAction.Interact, Key.E);
        Set(GameAction.Restart, Key.R);

        // === Debug ===
        Set(DebugAction.ToggleGizmos, Key.F1);
        Set(DebugAction.ToggleProfiler, Key.F2);
        Set(DebugAction.ToggleGrid, Key.F3);
        Set(DebugAction.ToggleTimeline, Key.F4);
        Set(DebugAction.ToggleConsole, Key.F9);
        Set(DebugAction.ToggleWatch, Key.F11);
        Set(DebugAction.ToggleAssetBrowser, Key.F12);
        // F6, F7, F8, F10 — зарезервированы под inspector / navgrid
    }

    // Game
    public void Set(GameAction a, Key k) => _game[a] = k;
    public Key Get(GameAction a) => _game.GetValueOrDefault(a, Key.Unknown);
    public bool Has(GameAction a) => _game.ContainsKey(a);
    public IReadOnlyDictionary<GameAction, Key> AllGame => _game;

    // Debug
    public void Set(DebugAction a, Key k) => _debug[a] = k;
    public Key Get(DebugAction a) => _debug.GetValueOrDefault(a, Key.Unknown);
    public bool Has(DebugAction a) => _debug.ContainsKey(a);
    public IReadOnlyDictionary<DebugAction, Key> AllDebug => _debug;

    public void Print()
    {
        MyEngine.Diagnostics.Log.Info("Bindings", "=== Game Actions ===");
        foreach (var kv in _game)
            MyEngine.Diagnostics.Log.Info("Bindings", $"  {kv.Key,-12} → {kv.Value}");

        MyEngine.Diagnostics.Log.Info("Bindings", "=== Debug Actions ===");
        foreach (var kv in _debug)
            MyEngine.Diagnostics.Log.Info("Bindings", $"  {kv.Key,-22} → {kv.Value}");
    }
}