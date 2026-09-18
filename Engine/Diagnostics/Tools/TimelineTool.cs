using MyEngine.InputEngine;
using Silk.NET.Input;

namespace MyEngine.Diagnostics.Tools;

public sealed class TimelineTool : IDebugTool
{
    private readonly PerfTimelineWindow _window;

    public string Name => "Timeline";
    public bool Visible { get => _window.Visible; set => _window.Visible = value; }
    public PerfTimelineWindow Window => _window;

    public TimelineTool(PerfTimelineWindow window) => _window = window;

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumePressed(Key.F4))
            _window.Visible = !_window.Visible;
    }

    public void Draw() => _window.Draw();
}