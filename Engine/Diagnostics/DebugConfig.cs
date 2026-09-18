namespace MyEngine.Diagnostics;

public static class DebugConfig
{
#if DEBUG
    public const bool Available = true;
#else
    public const bool Available = false;
#endif

    public static bool ShowGizmos = false; //Press F1
    public static bool ShowProfiler = false; // Press F2
    public static bool ShowGrid = false;   // Press F3
    public static bool ShowTimeline = false;
}