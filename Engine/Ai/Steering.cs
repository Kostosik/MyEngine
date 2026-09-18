using System.Numerics;

namespace MyEngine.Ai;

/// <summary>
/// Функции "желаемой силы" — куда объект хочет двигаться.
/// Все возвращают направление (нормализованное или с magnitude),
/// комбинируются весами в вызывающем коде.
/// </summary>
public static class Steering
{
    /// <summary>Движение к цели на полной скорости.</summary>
    public static Vector2 Seek(Vector2 position, Vector2 target, float maxSpeed)
    {
        var desired = target - position;
        if (desired.LengthSquared() < 0.0001f) return Vector2.Zero;
        return Vector2.Normalize(desired) * maxSpeed;
    }

    /// <summary>Движение от угрозы.</summary>
    public static Vector2 Flee(Vector2 position, Vector2 threat, float maxSpeed)
    {
        var desired = position - threat;
        if (desired.LengthSquared() < 0.0001f) return Vector2.Zero;
        return Vector2.Normalize(desired) * maxSpeed;
    }

    /// <summary>
    /// Движение к цели с замедлением на подходе.
    /// Внутри slowRadius скорость линейно падает до нуля.
    /// </summary>
    public static Vector2 Arrive(
        Vector2 position, Vector2 target,
        float maxSpeed, float slowRadius = 100f)
    {
        var toTarget = target - position;
        float dist = toTarget.Length();
        if (dist < 0.001f) return Vector2.Zero;

        // Линейное замедление в slowRadius
        float speed = dist < slowRadius
            ? maxSpeed * (dist / slowRadius)
            : maxSpeed;

        return Vector2.Normalize(toTarget) * speed;
    }

    /// <summary>
    /// Отталкивание от соседей — чтобы не толпились в одну точку.
    /// neighbors — список позиций всех соседей (обычно рядом стоящих врагов).
    /// </summary>
    public static Vector2 Separation(
        Vector2 position, IEnumerable<Vector2> neighbors,
        float desiredDistance, float maxSpeed)
    {
        var push = Vector2.Zero;
        int count = 0;

        foreach (var n in neighbors)
        {
            var diff = position - n;
            float dist = diff.Length();
            if (dist < 0.001f || dist > desiredDistance) continue;

            // Чем ближе — тем сильнее отталкивание
            float strength = (desiredDistance - dist) / desiredDistance;
            push += Vector2.Normalize(diff) * strength;
            count++;
        }

        if (count == 0) return Vector2.Zero;
        return Vector2.Normalize(push) * maxSpeed;
    }

    /// <summary>
    /// Обход ближайших препятствий (стен).
    /// obstacles — AABB-ы рядом, lookAhead — на сколько вперёд смотреть.
    /// Возвращает силу, перпендикулярную движению — чтобы объехать.
    /// </summary>
    public static Vector2 AvoidObstacles(
        Vector2 position, Vector2 velocity,
        IReadOnlyList<(Vector2 center, Vector2 size)> obstacles,
        float lookAhead, float maxSpeed)
    {
        if (velocity.LengthSquared() < 0.001f) return Vector2.Zero;

        var forward = Vector2.Normalize(velocity);
        var ahead = position + forward * lookAhead;
        var steer = Vector2.Zero;
        int count = 0;

        foreach (var (center, size) in obstacles)
        {
            var half = size * 0.5f;
            var min = center - half;
            var max = center + half;

            // Проверяем пересечение отрезка [position, ahead] с AABB.
            // Используем slab method — быстрый и надёжный.
            if (!SegmentIntersectsAabb(position, ahead, min, max))
                continue;

            // Отталкиваемся от центра препятствия
            var away = position - center;
            if (away.LengthSquared() < 0.001f)
                away = new Vector2(-forward.Y, forward.X); // перпендикуляр

            steer += Vector2.Normalize(away);
            count++;
        }

        if (count == 0) return Vector2.Zero;
        return Vector2.Normalize(steer) * maxSpeed;
    }

    /// <summary>Пересекает ли отрезок [a, b] прямоугольник [min, max]. Slab method.</summary>
    private static bool SegmentIntersectsAabb(Vector2 a, Vector2 b, Vector2 min, Vector2 max)
    {
        var dir = b - a;
        float tMin = 0f;
        float tMax = 1f;

        // Ось X
        if (MathF.Abs(dir.X) < 0.0001f)
        {
            if (a.X < min.X || a.X > max.X) return false;
        }
        else
        {
            float t1 = (min.X - a.X) / dir.X;
            float t2 = (max.X - a.X) / dir.X;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);
            if (tMin > tMax) return false;
        }

        // Ось Y
        if (MathF.Abs(dir.Y) < 0.0001f)
        {
            if (a.Y < min.Y || a.Y > max.Y) return false;
        }
        else
        {
            float t1 = (min.Y - a.Y) / dir.Y;
            float t2 = (max.Y - a.Y) / dir.Y;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);
            if (tMin > tMax) return false;
        }

        return true;
    }

    /// <summary>
    /// Случайное блуждание — для idle-поведения, чтобы враг не стоял столбом.
    /// </summary>
    public static Vector2 Wander(
        Vector2 position, float radius, float jitter,
        ref float wanderAngle, ref float wanderTimer, float dt)
    {
        wanderTimer -= dt;
        if (wanderTimer <= 0)
        {
            wanderAngle = (float)(Random.Shared.NextDouble() * System.Math.Tau);
            wanderTimer = 0.5f + (float)Random.Shared.NextDouble() * 1.5f;
        }

        return new Vector2(
            MathF.Cos(wanderAngle) * radius,
            MathF.Sin(wanderAngle) * radius);
    }
}