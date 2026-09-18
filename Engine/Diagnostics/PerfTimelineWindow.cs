using ImGuiNET;
using MyEngine.Threading;
using System.Diagnostics;
using System.Numerics;

namespace MyEngine.Diagnostics;

public sealed class PerfTimelineWindow
{
    private readonly JobProfiler _profiler;
    private readonly JobSystem _jobs;
    private readonly JobEvent[] _snapshot = new JobEvent[4096];

    private float _scopeMs = 100f;
    private int _rangeIndex = 1;
    private static readonly float[] Ranges = { 50f, 100f, 250f, 500f, 1000f, 2000f, 3000f, 4000f};

    public bool Visible { get; set; } = false;

    public PerfTimelineWindow(JobProfiler profiler, JobSystem jobs)
    {
        _profiler = profiler;
        _jobs = jobs;
    }

    public void Draw()
    {
        if (!Visible) return;

        var io = ImGui.GetIO();
        var size = new Vector2(760, 280);
        ImGui.SetNextWindowPos(
            new Vector2(io.DisplaySize.X * 0.5f - size.X * 0.5f,
                        io.DisplaySize.Y - size.Y - 20),
            ImGuiCond.Always);
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);

        ImGui.Begin("timeline",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoMove);

        // Управление
        ImGui.Text($"Scope: {_scopeMs:F0} ms");
        ImGui.SameLine();
        if (ImGui.Button("<")) _rangeIndex = System.Math.Max(0, _rangeIndex - 1);
        ImGui.SameLine();
        if (ImGui.Button(">")) _rangeIndex = System.Math.Min(Ranges.Length - 1, _rangeIndex + 1);
        _scopeMs = Ranges[_rangeIndex];

        ImGui.SameLine();
        int count = _profiler.Snapshot(_snapshot);
        ImGui.TextDisabled($"events: {count}");

        ImGui.Separator();

        // Область рисования
        var canvasOrigin = ImGui.GetCursorScreenPos();
        var canvasSize = new Vector2(
            ImGui.GetContentRegionAvail().X,
            ImGui.GetContentRegionAvail().Y);
        var dl = ImGui.GetWindowDrawList();

        long nowTicks = Stopwatch.GetTimestamp();
        long scopeTicks = (long)(_scopeMs / 1000.0 * Stopwatch.Frequency);
        long startTicks = nowTicks - scopeTicks;

        // Соберём уникальные worker id
        var workers = new SortedSet<int>();
        workers.Add(_profiler.MainThreadId);
        for (int i = 0; i < count; i++)
        {
            if (_snapshot[i].EndTicks < startTicks) continue;
            workers.Add(_snapshot[i].WorkerId);
        }

        var workerList = workers.ToArray();
        int rows = workerList.Length;
        if (rows == 0) { ImGui.End(); return; }

        float rowH = MathF.Min(24f, canvasSize.Y / rows);

        // Отрисовка полосок
        for (int r = 0; r < rows; r++)
        {
            float y = canvasOrigin.Y + r * rowH;
            uint bgCol = workerList[r] == _profiler.MainThreadId
                ? ImGui.ColorConvertFloat4ToU32(new Vector4(0.25f, 0.2f, 0.1f, 1f))
                : ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.12f, 0.14f, 1f));

            dl.AddRectFilled(
                new Vector2(canvasOrigin.X, y),
                new Vector2(canvasOrigin.X + canvasSize.X, y + rowH - 1),
                bgCol);

            // Подпись
            string label = workerList[r] == _profiler.MainThreadId
                ? "main"
                : $"w{workerList[r] % 100}";
            dl.AddText(new Vector2(canvasOrigin.X + 4, y + 2),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1f)),
                label);
        }

        // События
        for (int i = 0; i < count; i++)
        {
            ref var ev = ref _snapshot[i];
            if (ev.EndTicks < startTicks) continue;
            if (ev.StartTicks > nowTicks) continue;

            int row = Array.IndexOf(workerList, ev.WorkerId);
            if (row < 0) continue;

            // Позиции
            float x0 = (float)((ev.StartTicks - startTicks) / (double)scopeTicks);
            float x1 = (float)((ev.EndTicks - startTicks) / (double)scopeTicks);
            x0 = System.Math.Clamp(x0, 0f, 1f);
            x1 = System.Math.Clamp(x1, 0f, 1f);

            if (x1 - x0 < 0.001f) x1 = x0 + 0.001f;

            float py = canvasOrigin.Y + row * rowH + 1;
            var p0 = new Vector2(canvasOrigin.X + x0 * canvasSize.X, py);
            var p1 = new Vector2(canvasOrigin.X + x1 * canvasSize.X, py + rowH - 3);

            uint col = ColorFromName(ev.Name);
            dl.AddRectFilled(p0, p1, col);
        }

        // Заголовок со шкалой
        ImGui.SetCursorScreenPos(new Vector2(canvasOrigin.X, canvasOrigin.Y - 16));
        for (int i = 0; i <= 4; i++)
        {
            float t = i / 4f;
            string s = $"{(int)(_scopeMs * (1 - t))}ms";
            dl.AddText(new Vector2(canvasOrigin.X + t * canvasSize.X - 14, canvasOrigin.Y - 16),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.55f, 0.55f, 0.6f, 1f)),
                s);
        }

        ImGui.End();
    }

    private static uint ColorFromName(string name)
    {
        int hash = 0;
        foreach (var c in name) hash = hash * 31 + c;
        float hue = (hash & 0xFFFF) / 65535f;

        // HSV → RGB (sat=0.6, val=0.9)
        float h = hue * 6f;
        int i = (int)h;
        float f = h - i;
        float p = 0.9f * (1 - 0.6f);
        float q = 0.9f * (1 - 0.6f * f);
        float t = 0.9f * (1 - 0.6f * (1 - f));

        (float r, float g, float b) = (i % 6) switch
        {
            0 => (0.9f, t, p),
            1 => (q, 0.9f, p),
            2 => (p, 0.9f, t),
            3 => (p, q, 0.9f),
            4 => (t, p, 0.9f),
            _ => (0.9f, p, q),
        };

        return ImGui.ColorConvertFloat4ToU32(new Vector4(r, g, b, 1f));
    }
}