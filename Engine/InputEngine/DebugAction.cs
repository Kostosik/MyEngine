namespace MyEngine.InputEngine;

/// <summary>
/// Debug-действия. Отдельно от GameAction — потому что это
/// инструменты разработчика, а не игровые команды.
/// В Release могут быть недоступны.
/// </summary>
public enum DebugAction
{
    ToggleGizmos,
    ToggleProfiler,
    ToggleGrid,
    ToggleTimeline,
    ToggleConsole,
    ToggleWatch,
    ToggleAssetBrowser,
    ToggleWorldInspector,
    ToggleEntityInspector,
    ToggleNavGridGrid,
    ToggleNavGridPaths,
}