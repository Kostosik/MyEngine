using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Math;
using System.Numerics;

namespace MyEngine.Physics;

/// <summary>
/// Поиск пересечений луча с AABB-коллайдерами.
///
/// Сейчас — простой перебор всех сущностей с Collider. Для 100–500
/// коллайдеров это быстро. Если понадобится больше — заменим
/// на grid-based DDA raycast (см. комментарий в RaycastSystem ниже).
///
/// Луч не проверяет триггеры отдельно — если хочешь игнорировать
/// триггеры, используй маску слоёв.
/// </summary>
public static class Raycast
{
    /// <summary>
    /// Найти ближайшее пересечение луча с коллайдерами в мире.
    /// </summary>
    /// <param name="world">Мир для поиска.</param>
    /// <param name="ray">Луч.</param>
    /// <param name="mask">
    /// Маска слоёв — рассматриваются только коллайдеры, у которых
    /// (Collider.Layer &amp; mask) != 0. Если mask == Layer.None,
    /// проверяются все коллайдеры.
    /// </param>
    /// <param name="ignore">Сущность, которую нужно игнорировать (обычно сам источник луча).</param>
    /// <returns>Ближайшее попадание или null, если ничего не найдено.</returns>
    public static RaycastHit? RaycastFirst(
        World world,
        Ray ray,
        Layer mask = Layer.None,
        Entity? ignore = null)
    {
        RaycastHit? best = null;
        float bestDist = float.MaxValue;

        foreach (var e in world.With<Collider>())
        {
            if (ignore != null && ReferenceEquals(e, ignore)) continue;

            var c = e.Get<Collider>()!;
            if (mask != Layer.None && (c.Layer & mask) == 0) continue;

            var t = e.Get<Transform>()!;
            var box = Aabb.FromCenterSize(t.Position, c.Size);

            if (RayAabb(ray, box, out float dist, out var normal))
            {
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new RaycastHit(e, ray.PointAt(dist), normal, dist);
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Все пересечения луча с коллайдерами, отсортированные по расстоянию.
    /// Полезно для пробивающих лучей (например, "урон всем в линии").
    /// </summary>
    public static void RaycastAll(
        World world,
        Ray ray,
        List<RaycastHit> results,
        Layer mask = Layer.None,
        Entity? ignore = null)
    {
        results.Clear();

        foreach (var e in world.With<Collider>())
        {
            if (ignore != null && ReferenceEquals(e, ignore)) continue;

            var c = e.Get<Collider>()!;
            if (mask != Layer.None && (c.Layer & mask) == 0) continue;

            var t = e.Get<Transform>()!;
            var box = Aabb.FromCenterSize(t.Position, c.Size);

            if (RayAabb(ray, box, out float dist, out var normal))
                results.Add(new RaycastHit(e, ray.PointAt(dist), normal, dist));
        }

        results.Sort((a, b) => a.Distance.CompareTo(b.Distance));
    }

    /// <summary>
    /// Пересечение луча с AABB. Slab method — классический алгоритм.
    /// Работает для 2D (третья ось игнорируется).
    /// </summary>
    private static bool RayAabb(
        in Ray ray,
        in Aabb box,
        out float distance,
        out Vector2 normal)
    {
        distance = 0;
        normal = Vector2.Zero;

        float tMin = 0f;
        float tMax = ray.MaxDistance;
        Vector2 hitNormal = Vector2.Zero;

        // Ось X
        if (!IntersectAxis(
            ray.Origin.X, ray.Direction.X,
            box.Min.X, box.Max.X,
            new Vector2(-1, 0), new Vector2(1, 0),
            ref tMin, ref tMax, ref hitNormal))
            return false;

        // Ось Y
        if (!IntersectAxis(
            ray.Origin.Y, ray.Direction.Y,
            box.Min.Y, box.Max.Y,
            new Vector2(0, -1), new Vector2(0, 1),
            ref tMin, ref tMax, ref hitNormal))
            return false;

        if (tMax < 0f) return false;

        distance = tMin;
        normal = hitNormal;
        return true;
    }

    private static bool IntersectAxis(
        float origin, float dir,
        float min, float max,
        Vector2 negNormal, Vector2 posNormal,
        ref float tMin, ref float tMax, ref Vector2 normal)
    {
        const float epsilon = 1e-6f;

        if (MathF.Abs(dir) < epsilon)
        {
            // Луч параллелен оси — он либо внутри полосы, либо никогда не попадёт
            if (origin < min || origin > max) return false;
            return true;
        }

        float invDir = 1f / dir;
        float t1 = (min - origin) * invDir;
        float t2 = (max - origin) * invDir;

        Vector2 normalCandidate;
        if (t1 < t2)
        {
            normalCandidate = negNormal; // вход через min
        }
        else
        {
            (t1, t2) = (t2, t1);
            normalCandidate = posNormal; // вход через max
        }

        if (t1 > tMin)
        {
            tMin = t1;
            normal = normalCandidate;
        }

        if (t2 < tMax) tMax = t2;

        return tMin <= tMax;
    }
}