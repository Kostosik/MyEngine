using ImGuiNET;
using MyEngine.Rendering;
using System.Diagnostics;
using System.Numerics;

namespace MyEngine.Diagnostics;

public sealed class DebugDraw
{
    private readonly Camera2D _camera;

    public DebugDraw(Camera2D camera) => _camera = camera;

    [Conditional("DEBUG")]
    public void Box(Vector2 center, Vector2 size, Vector4 color, bool filled = false)
    {
        var io = ImGui.GetIO();
        float w = io.DisplaySize.X, h = io.DisplaySize.Y;

        var min = center - size * 0.5f;
        var max = center + size * 0.5f;

        var a = _camera.WorldToScreen(min, w, h);
        var b = _camera.WorldToScreen(new Vector2(max.X, min.Y), w, h);
        var c = _camera.WorldToScreen(max, w, h);
        var d = _camera.WorldToScreen(new Vector2(min.X, max.Y), w, h);

        uint col = ImGui.ColorConvertFloat4ToU32(color);
        var dl = ImGui.GetForegroundDrawList();

        if (filled)
            dl.AddQuadFilled(a, b, c, d, col);
        else
            dl.AddQuad(a, b, c, d, col, 1.5f);
    }

    [Conditional("DEBUG")]
    public void Circle(Vector2 center, float worldRadius, Vector4 color, int segments = 32)
    {
        var io = ImGui.GetIO();
        float w = io.DisplaySize.X, h = io.DisplaySize.Y;
        uint col = ImGui.ColorConvertFloat4ToU32(color);

        var screen = _camera.WorldToScreen(center, w, h);
        float r = worldRadius * _camera.Zoom;

        ImGui.GetForegroundDrawList().AddCircle(screen, r, col, segments, 1.5f);
    }

    [Conditional("DEBUG")]
    public void Line(Vector2 a, Vector2 b, Vector4 color, float thickness = 1.5f)
    {
        var io = ImGui.GetIO();
        float w = io.DisplaySize.X, h = io.DisplaySize.Y;

        var sa = _camera.WorldToScreen(a, w, h);
        var sb = _camera.WorldToScreen(b, w, h);
        uint col = ImGui.ColorConvertFloat4ToU32(color);

        ImGui.GetForegroundDrawList().AddLine(sa, sb, col, thickness);
    }

    [Conditional("DEBUG")]
    public void Arrow(Vector2 from, Vector2 to, Vector4 color)
    {
        Line(from, to, color, 2f);

        var dir = to - from;
        if (dir.LengthSquared() < 0.01f) return;
        dir = Vector2.Normalize(dir);

        float headLen = MathF.Min(12f, Vector2.Distance(from, to) * 0.4f);
        var left = new Vector2(
            dir.X * MathF.Cos(0.5f) - dir.Y * MathF.Sin(0.5f),
            dir.X * MathF.Sin(0.5f) + dir.Y * MathF.Cos(0.5f));
        var right = new Vector2(
            dir.X * MathF.Cos(-0.5f) - dir.Y * MathF.Sin(-0.5f),
            dir.X * MathF.Sin(-0.5f) + dir.Y * MathF.Cos(-0.5f));

        Line(to, to - left * headLen, color, 2f);
        Line(to, to - right * headLen, color, 2f);
    }

    [Conditional("DEBUG")]
    public void WorldText(Vector2 worldPos, string text, Vector4 color)
    {
        var io = ImGui.GetIO();
        float w = io.DisplaySize.X, h = io.DisplaySize.Y;
        var screen = _camera.WorldToScreen(worldPos, w, h);
        uint col = ImGui.ColorConvertFloat4ToU32(color);

        ImGui.GetForegroundDrawList().AddText(screen, col, text);
    }

    [Conditional("DEBUG")]
    public void Grid(Vector2 center, float extent, float step, Vector4 color)
    {
        float x0 = MathF.Floor((center.X - extent) / step) * step;
        float x1 = center.X + extent;
        float y0 = MathF.Floor((center.Y - extent) / step) * step;
        float y1 = center.Y + extent;

        for (float x = x0; x <= x1; x += step)
            Line(new Vector2(x, y0), new Vector2(x, y1), color, 1f);
        for (float y = y0; y <= y1; y += step)
            Line(new Vector2(x0, y), new Vector2(x1, y), color, 1f);
    }
}