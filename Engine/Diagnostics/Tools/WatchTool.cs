using MyEngine.InputEngine;
using Silk.NET.Input;

namespace MyEngine.Diagnostics.Tools;

public sealed class WatchTool : IDebugTool
{
    private readonly WatchWindow _window = new();

    public string Name => "Watch";
    public bool Visible { get => _window.Visible; set => _window.Visible = value; }
    public WatchWindow Window => _window;

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumeDebugPressed(DebugAction.ToggleWatch))
            _window.Toggle();
    }

    public void Draw() => _window.Draw();
}