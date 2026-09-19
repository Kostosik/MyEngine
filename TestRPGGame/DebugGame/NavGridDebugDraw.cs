using ImGuiNET;
using MyEngine.Ai;
using MyEngine.Ecs;
using MyEngine.Components;
using MyEngine.Rendering;
using System.Numerics;
using TestRPGGame.Ai;

namespace TestRPGGame.DebugGame;

public static class NavGridDebugDraw
{
    public static bool ShowGrid = false;
    public static bool ShowPaths = true;
    public static bool ShowMemory = true;

    public static void Draw(
        World world, NavGrid? navGrid, Camera2D camera,
        int screenWidth, int screenHeight)
    {
        var dl = ImGui.GetForegroundDrawList();

        // 1. Сетка — рисуем клетки
        if (ShowGrid && navGrid != null)
        {
            for (int y = 0; y < navGrid.Height; y++)
            {
                for (int x = 0; x < navGrid.Width; x++)
                {
                    var center = navGrid.CellToWorld(x, y);
                    float half = navGrid.CellSize * 0.5f;

                    var p0 = camera.WorldToScreen(
                        center + new Vector2(-half, -half), screenWidth, screenHeight);
                    var p1 = camera.WorldToScreen(
                        center + new Vector2(half, half), screenWidth, screenHeight);

                    // Отсеиваем далёкие клетки за границами экрана
                    if (p1.X < 0 || p0.X > screenWidth) continue;
                    if (p1.Y < 0 || p0.Y > screenHeight) continue;

                    bool walkable = navGrid.IsWalkable(x, y);
                    uint col = walkable
                        ? ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.9f, 0.2f, 0.08f))
                        : ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.2f, 0.2f, 0.25f));

                    dl.AddRectFilled(p0, p1, col);
                }
            }
        }

        // 2. Пути врагов + память
        foreach (var e in world.With<EnemyTag>())
        {
            var bc = e.Get<BehaviorComponent>();
            if (bc?.Behavior is not SlimeBehavior slime) continue;

            // Путь — линия через waypoints
            if (ShowPaths && slime.CurrentPath != null && slime.CurrentPath.Count > 1)
            {
                uint pathCol = ImGui.ColorConvertFloat4ToU32(
                    new Vector4(1f, 0.8f, 0.2f, 0.9f));

                var prev = camera.WorldToScreen(
                    slime.CurrentPath[0], screenWidth, screenHeight);

                for (int i = 1; i < slime.CurrentPath.Count; i++)
                {
                    var cur = camera.WorldToScreen(
                        slime.CurrentPath[i], screenWidth, screenHeight);
                    dl.AddLine(prev, cur, pathCol, 2f);
                    prev = cur;
                }

                // Точки waypoint
                foreach (var wp in slime.CurrentPath)
                {
                    var sp = camera.WorldToScreen(wp, screenWidth, screenHeight);
                    dl.AddCircleFilled(sp, 2.5f, pathCol);
                }
            }

            // Память о игроке
            if (ShowMemory && slime.LastKnownPlayerPos.HasValue)
            {
                var memPos = camera.WorldToScreen(
                    slime.LastKnownPlayerPos.Value, screenWidth, screenHeight);

                uint memCol = slime.CanSeePlayer
                    ? ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 0.3f, 1f))
                    : ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.4f, 1f, 0.7f));

                dl.AddCircle(memPos, 8f, memCol, 12, 2f);
                dl.AddCircleFilled(memPos, 2f, memCol);
            }
        }
    }
}