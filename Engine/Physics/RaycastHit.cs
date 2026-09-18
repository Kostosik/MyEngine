using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Physics;

/// <summary>
/// Результат попадания луча в коллайдер.
/// </summary>
public readonly struct RaycastHit
{
    /// <summary>Сущность, в которую попал луч.</summary>
    public readonly Entity Entity;

    /// <summary>Точка попадания в мировых координатах.</summary>
    public readonly Vector2 Point;

    /// <summary>
    /// Нормаль поверхности в точке попадания.
    /// Направлена наружу от коллайдера.
    /// </summary>
    public readonly Vector2 Normal;

    /// <summary>Расстояние от начала луча до точки попадания.</summary>
    public readonly float Distance;

    public RaycastHit(Entity entity, Vector2 point, Vector2 normal, float distance)
    {
        Entity = entity;
        Point = point;
        Normal = normal;
        Distance = distance;
    }
}