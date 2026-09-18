using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Math;
using MyEngine.Spatial;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Разрешение столкновений. Два режима:
///
/// TopDown — минимальное проникновение по любой оси. Для RPG.
/// Platformer — сначала X, потом Y. Позволяет скользить по стенам и стоять на полу.
///
/// Автоматически выбирается: если в мире есть сущности с PlatformerController,
/// используется Platformer-режим. Иначе TopDown.
/// </summary>
public sealed class CollisionSystem : ISystem
{
    private readonly SpatialHash<Entity> _staticHash = new(cellSize: 128f);
    private readonly List<Entity> _queryBuffer = new(16);

    public void Update(World world, float dt)
    {
        // Собираем статичные коллайдеры
        _staticHash.Clear();
        foreach (var w in world.With<Collider>())
        {
            var c = w.Get<Collider>()!;
            if (!c.IsStatic || c.IsTrigger) continue;
            var t = w.Get<Transform>()!;
            _staticHash.Insert(w, Aabb.FromCenterSize(t.Position, c.Size));
        }

        // Обрабатываем динамические
        foreach (var e in world.With<Collider>())
        {
            var c = e.Get<Collider>()!;
            if (c.IsStatic || c.IsTrigger) continue;
            var t = e.Get<Transform>()!;

            // Определяем режим — по наличию PlatformerController
            bool platformer = e.Has<PlatformerController>();

            if (platformer)
                ResolvePlatformer(e, c, t);
            else
                ResolveTopDown(c, t);
        }
    }

    private void ResolveTopDown(Collider c, Transform t)
    {
        var selfBox = Aabb.FromCenterSize(t.Position, c.Size);
        var searchBox = new Aabb(selfBox.Min - new Vector2(2, 2), selfBox.Max + new Vector2(2, 2));
        _staticHash.Query(searchBox, _queryBuffer);

        foreach (var wall in _queryBuffer)
        {
            var wc = wall.Get<Collider>()!;
            if ((c.CollidesWith & wc.Layer) == 0) continue;

            var wt = wall.Get<Transform>()!;
            var a = Aabb.FromCenterSize(t.Position, c.Size);
            var b = Aabb.FromCenterSize(wt.Position, wc.Size);
            if (!a.Intersects(b)) continue;

            var ov = a.Overlap(b);
            if (ov.X < ov.Y)
                t.Position += new Vector2(a.Min.X < b.Min.X ? -ov.X : ov.X, 0);
            else
                t.Position += new Vector2(0, a.Min.Y < b.Min.Y ? -ov.Y : ov.Y);
        }
    }

    private void ResolvePlatformer(Entity e, Collider c, Transform t)
    {
        var ctrl = e.Get<PlatformerController>();
        var vel = e.Get<Velocity>();
        bool wasGrounded = ctrl?.Grounded ?? false;
        bool groundedNow = false;

        // === X ===
        {
            var selfBox = Aabb.FromCenterSize(t.Position, c.Size);
            var searchBox = new Aabb(selfBox.Min - new Vector2(2, 2), selfBox.Max + new Vector2(2, 2));
            _staticHash.Query(searchBox, _queryBuffer);

            foreach (var wall in _queryBuffer)
            {
                var wc = wall.Get<Collider>()!;
                if ((c.CollidesWith & wc.Layer) == 0) continue;

                var wt = wall.Get<Transform>()!;
                var a = Aabb.FromCenterSize(t.Position, c.Size);
                var b = Aabb.FromCenterSize(wt.Position, wc.Size);
                if (!a.Intersects(b)) continue;

                var ov = a.Overlap(b);
                float push = a.Min.X < b.Min.X ? -ov.X : ov.X;
                t.Position += new Vector2(push, 0);
                if (vel != null) vel.Value.X = 0;
            }
        }

        // === Y ===
        {
            var selfBox = Aabb.FromCenterSize(t.Position, c.Size);
            var searchBox = new Aabb(selfBox.Min - new Vector2(2, 2), selfBox.Max + new Vector2(2, 2));
            _staticHash.Query(searchBox, _queryBuffer);

            foreach (var wall in _queryBuffer)
            {
                var wc = wall.Get<Collider>()!;
                if ((c.CollidesWith & wc.Layer) == 0) continue;

                // === One-way check ===
                if (wall.Has<OneWayPlatform>())
                {
                    var wt0 = wall.Get<Transform>()!;
                    // Если игрок движется вверх — пропускаем платформу
                    if (vel != null && vel.Value.Y < 0f) continue;
                    // Если центр игрока ниже центра платформы — пропускаем
                    if (t.Position.Y > wt0.Position.Y) continue;
                }

                var wt = wall.Get<Transform>()!;
                var a = Aabb.FromCenterSize(t.Position, c.Size);
                var b = Aabb.FromCenterSize(wt.Position, wc.Size);
                if (!a.Intersects(b)) continue;

                var ov = a.Overlap(b);
                float push = a.Min.Y < b.Min.Y ? -ov.Y : ov.Y;
                t.Position += new Vector2(0, push);

                if (push < 0)
                {
                    // Стоим на земле
                    groundedNow = true;
                    if (vel != null && vel.Value.Y > 0) vel.Value.Y = 0;
                }
                else
                {
                    // Ударились головой
                    if (vel != null && vel.Value.Y < 0) vel.Value.Y = 0;
                }
            }
        }

        if (ctrl != null)
            ctrl.Grounded = groundedNow;

        // Дополнительный «щуп» на 2 пикселя вниз — ловит стояние на границе
        if (!groundedNow && ctrl != null)
        {
            var footProbe = Aabb.FromCenterSize(
                t.Position + new Vector2(0, c.Size.Y * 0.5f + 1f),
                new Vector2(c.Size.X * 0.8f, 2f));

            _staticHash.Query(footProbe, _queryBuffer);

            foreach (var wall in _queryBuffer)
            {
                var wc = wall.Get<Collider>()!;
                if ((c.CollidesWith & wc.Layer) == 0) continue;

                var wt = wall.Get<Transform>()!;
                var b = Aabb.FromCenterSize(wt.Position, wc.Size);
                if (footProbe.Intersects(b))
                {
                    ctrl.Grounded = true;
                    break;
                }
            }
        }
    }
}