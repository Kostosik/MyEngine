using MyEngine.Diagnostics.Console;
using MyEngine.InputEngine;
using Silk.NET.Input;

namespace MyEngine.Diagnostics.Tools;

public sealed class ConsoleTool : IDebugTool, IBlockingTool, IDisposable
{
    private readonly ConsoleSystem _console;
    private readonly ConsoleWindow _window;

    public string Name => "Console";
    public bool Visible { get => _window.Visible; set => _window.Visible = value; }
    public bool BlocksGameInput => _window.Visible;

    public ConsoleSystem Console => _console;
    public ConsoleWindow Window => _window;

    public ConsoleTool()
    {
        _console = new ConsoleSystem();
        BuiltInCommands.Register(_console);
        _window = new ConsoleWindow(_console);
        _console.WriteInfo("Console ready. Type 'help' for commands.");
    }

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumeDebugPressed(DebugAction.ToggleConsole))
            _window.Toggle();
    }

    public void Draw() => _window.Draw();

    public void Dispose() { }
}