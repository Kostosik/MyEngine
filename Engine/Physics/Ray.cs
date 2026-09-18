using System.Numerics;

namespace MyEngine.Physics;

/// <summary>
/// Луч: начало, направление и максимальная длина.
/// Направление должно быть нормализовано — иначе Distance в RaycastHit
/// будет в "единицах направления", а не в пикселях.
/// </summary>
public readonly struct Ray
{
    public readonly Vector2 Origin;
    public readonly Vector2 Direction; // нормализованный
    public readonly float MaxDistance;

    public Ray(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue)
    {
        Origin = origin;
        Direction = direction.LengthSquared() > 0.0001f
            ? Vector2.Normalize(direction)
            : new Vector2(1, 0); // защита от нулевого направления
        MaxDistance = maxDistance;
    }

    public Vector2 PointAt(float distance)
        => Origin + Direction * distance;
}