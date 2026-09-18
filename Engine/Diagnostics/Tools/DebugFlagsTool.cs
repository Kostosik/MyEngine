using MyEngine.InputEngine;
using Silk.NET.Input;

namespace MyEngine.Diagnostics.Tools;

/// <summary>
/// Переключение глобальных debug-флагов (gizmo, grid, etc.).
/// Не имеет окна — только реагирует на клавиши.
/// </summary>
public sealed class DebugFlagsTool : IDebugTool
{
    public string Name => "DebugFlags";

    // Нет окна — Visible не используется
    public bool Visible { get => false; set { } }

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumeDebugPressed(DebugAction.ToggleGizmos))
            DebugConfig.ShowGizmos = !DebugConfig.ShowGizmos;

        if (input.ConsumeDebugPressed(DebugAction.ToggleGrid))
            DebugConfig.ShowGrid = !DebugConfig.ShowGrid;

        if (input.ConsumeDebugPressed(DebugAction.ToggleTimeline))
            DebugConfig.ShowTimeline = !DebugConfig.ShowTimeline;
    }

    public void Draw() { }   // окна нет — рисовать нечего
}