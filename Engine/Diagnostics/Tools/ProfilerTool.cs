using MyEngine.InputEngine;
using Silk.NET.Input;

namespace MyEngine.Diagnostics.Tools;

public sealed class ProfilerTool : IDebugTool
{
    private readonly Profiler _profiler;
    private readonly ProfilerWindow _window;

    public string Name => "Profiler";
    public bool Visible { get => _window.Visible; set => _window.Visible = value; }
    public Profiler Profiler => _profiler;
    public ProfilerWindow Window => _window;

    public ProfilerTool(Profiler profiler, ProfilerWindow window)
    {
        _profiler = profiler;
        _window = window;
    }

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumeDebugPressed(DebugAction.ToggleProfiler))
            _window.Visible = !_window.Visible;
    }

    public void Draw() => _window.Draw();
}