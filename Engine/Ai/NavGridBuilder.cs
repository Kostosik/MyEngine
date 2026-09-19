using MyEngine.Components;
using MyEngine.Math;
using System.Numerics;
using MyEngine.Ecs;

namespace MyEngine.Ai;

public static class NavGridBuilder
{
    public static NavGrid Build(World world, float cellSize, Vector2 mapMin, Vector2 mapMax)
    {
        int width = (int)MathF.Ceiling((mapMax.X - mapMin.X) / cellSize);
        int height = (int)MathF.Ceiling((mapMax.Y - mapMin.Y) / cellSize);
        var grid = new NavGrid(width, height, cellSize, mapMin);

        foreach (var e in world.With<Collider>())
        {
            var c = e.Get<Collider>()!;
            if (!c.IsStatic) continue;
            var t = e.Get<Transform>()!;

            // Блокируем ровно AABB стены, без раздутия
            var box = Aabb.FromCenterSize(t.Position, c.Size);
            grid.BlockAabb(box);
        }

        return grid;
    }
}