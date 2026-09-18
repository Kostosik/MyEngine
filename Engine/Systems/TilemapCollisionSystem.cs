using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Math;
using MyEngine.Tilemap;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Разрешает коллизии динамических сущностей с solid-тайлами.
/// Работает ПОСЛЕ обычного CollisionSystem.
///
/// Для каждой сущности с Collider и без IsStatic — берём тайлы рядом
/// и разрешаем пересечения как с AABB-стенами.
/// </summary>
public sealed class TilemapCollisionSystem : ISystem
{
    private readonly TilemapComponent _map;

    public SystemPhase Phase => SystemPhase.PostUpdate;
    public int Priority => 20;

    public TilemapCollisionSystem(TilemapComponent map) => _map = map;

    public void Update(World world, float dt)
    {
        foreach (var e in world.Query().With<Collider>().With<Transform>())
        {
            var c = e.Get<Collider>()!;
            if (c.IsStatic || c.IsTrigger) continue;

            var t = e.Get<Transform>()!;
            ResolveAgainstTiles(t, c);
        }
    }

    private void ResolveAgainstTiles(Transform t, Collider c)
    {
        // Позиция сущности в тайловых координатах
        var (cx, cy) = _map.WorldToTile(t.Position);

        // Размер сущности в тайлах + запас
        int extentX = (int)MathF.Ceiling(c.Size.X / _map.TileSize) + 1;
        int extentY = (int)MathF.Ceiling(c.Size.Y / _map.TileSize) + 1;

        // Обходим тайлы в радиусе
        for (int ty = cy - extentY; ty <= cy + extentY; ty++)
        {
            for (int tx = cx - extentX; tx <= cx + extentX; tx++)
            {
                if (tx < 0 || ty < 0 || tx >= _map.Width || ty >= _map.Height) continue;

                // Твёрдый ли тайл хоть на одном solid-слое?
                if (!IsSolidAt(tx, ty)) continue;

                // AABB тайла
                var tileMin = _map.TileToWorld(tx, ty);
                var tileMax = tileMin + new Vector2(_map.TileSize, _map.TileSize);

                // AABB сущности
                var selfBox = Aabb.FromCenterSize(t.Position, c.Size);
                var tileBox = new Aabb(tileMin, tileMax);

                if (!selfBox.Intersects(tileBox)) continue;

                // Выталкиваем по минимальному проникновению
                var ov = selfBox.Overlap(tileBox);
                if (ov.X < ov.Y)
                    t.Position += new Vector2(selfBox.Min.X < tileBox.Min.X ? -ov.X : ov.X, 0);
                else
                    t.Position += new Vector2(0, selfBox.Min.Y < tileBox.Min.Y ? -ov.Y : ov.Y);
            }
        }
    }

    private bool IsSolidAt(int tx, int ty)
    {
        foreach (var layer in _map.Layers)
        {
            if (!layer.Solid) continue;
            if (_map.GetTile(layer.Index, tx, ty) >= 0) return true;
        }
        return false;
    }
}