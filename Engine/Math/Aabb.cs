using System.Numerics;

namespace MyEngine.Math;

public readonly struct Aabb
{
    public readonly Vector2 Min;
    public readonly Vector2 Max;

    public Aabb(Vector2 min, Vector2 max) { Min = min; Max = max; }

    public static Aabb FromCenterSize(Vector2 center, Vector2 size)
        => new(center - size * 0.5f, center + size * 0.5f);

    public bool Intersects(in Aabb o)
        => Min.X < o.Max.X && Max.X > o.Min.X
        && Min.Y < o.Max.Y && Max.Y > o.Min.Y;

    public Vector2 Overlap(in Aabb o)
    {
        float ox = System.Math.Min(Max.X, o.Max.X) - System.Math.Max(Min.X, o.Min.X);
        float oy = System.Math.Min(Max.Y, o.Max.Y) - System.Math.Max(Min.Y, o.Min.Y);
        return new Vector2(ox, oy);
    }
}