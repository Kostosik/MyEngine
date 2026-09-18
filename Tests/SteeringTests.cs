using MyEngine.Ai;
using System.Numerics;

namespace MyEngine.Tests;

public class SteeringTests
{
    [Fact]
    public void Seek_PointsTowardTarget()
    {
        var v = Steering.Seek(new Vector2(0, 0), new Vector2(100, 0), maxSpeed: 50f);
        Assert.Equal(50f, v.X, 3);
        Assert.Equal(0f, v.Y, 3);
    }

    [Fact]
    public void Seek_AtTarget_ReturnsZero()
    {
        var v = Steering.Seek(new Vector2(10, 10), new Vector2(10, 10), maxSpeed: 50f);
        Assert.Equal(Vector2.Zero, v);
    }

    [Fact]
    public void Arrive_SlowsDownNearTarget()
    {
        // На расстоянии 50 при slowRadius=100 → скорость 50% от max
        var v = Steering.Arrive(new Vector2(0, 0), new Vector2(50, 0), maxSpeed: 100f, slowRadius: 100f);
        Assert.Equal(50f, v.X, 3);
    }

    [Fact]
    public void Separation_PushesAway()
    {
        var v = Steering.Separation(
            position: new Vector2(0, 0),
            neighbors: new[] { new Vector2(5, 0), new Vector2(0, 5) },
            desiredDistance: 20f,
            maxSpeed: 100f);

        // Отталкивание должно быть "от соседей" — в сторону (-x, -y)
        Assert.True(v.X < 0);
        Assert.True(v.Y < 0);
    }

    [Fact]
    public void AvoidObstacles_SteersAround()
    {
        // Летим вправо, впереди стена
        var v = Steering.AvoidObstacles(
            position: new Vector2(0, 0),
            velocity: new Vector2(100, 0),
            obstacles: new[] { (new Vector2(50, 0), new Vector2(30, 30)) },
            lookAhead: 80f,
            maxSpeed: 100f);

        Assert.NotEqual(Vector2.Zero, v);
    }
}