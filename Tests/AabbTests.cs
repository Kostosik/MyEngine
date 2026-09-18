using MyEngine.Math;
using System.Numerics;

namespace MyEngine.Tests;

public class AabbTests
{
    [Fact]
    public void Overlap_ReturnsExpected()
    {
        var a = Aabb.FromCenterSize(new Vector2(0, 0), new Vector2(10, 10));
        var b = Aabb.FromCenterSize(new Vector2(8, 0), new Vector2(10, 10));
        Assert.True(a.Intersects(b));
        var ov = a.Overlap(b);
        Assert.Equal(2f, ov.X, 3);
    }

    [Fact]
    public void NotIntersecting()
    {
        var a = Aabb.FromCenterSize(new Vector2(0, 0), new Vector2(10, 10));
        var b = Aabb.FromCenterSize(new Vector2(100, 0), new Vector2(10, 10));
        Assert.False(a.Intersects(b));
    }

    [Fact]
    public void Touching_DoesNotIntersect()
    {
        // Касание по границе не считается пересечением
        var a = Aabb.FromCenterSize(new Vector2(0, 0), new Vector2(10, 10));
        var b = Aabb.FromCenterSize(new Vector2(10, 0), new Vector2(10, 10));
        Assert.False(a.Intersects(b));
    }
}