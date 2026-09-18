using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Physics;
using System.Numerics;

namespace MyEngine.Tests;

public class RaycastTests
{
    private static Entity CreateBox(World w, Vector2 pos, Vector2 size, Layer layer = Layer.Wall)
    {
        var e = w.Create();
        e.Add(new Transform { Position = pos });
        e.Add(new Collider
        {
            Size = size,
            IsStatic = true,
            Layer = layer,
            CollidesWith = Layer.Player | Layer.Enemy
        });
        return e;
    }

    [Fact]
    public void RayHitsBoxInFront()
    {
        var world = new World();
        CreateBox(world, new Vector2(50, 0), new Vector2(20, 20));

        var hit = Raycast.RaycastFirst(
            world, new Ray(Vector2.Zero, new Vector2(1, 0)));

        Assert.NotNull(hit);
        Assert.Equal(40f, hit!.Value.Distance, 1); // 50 - 10 = 40
        Assert.Equal(new Vector2(-1, 0), hit.Value.Normal);
    }

    [Fact]
    public void RayMissesBoxOffAxis()
    {
        var world = new World();
        CreateBox(world, new Vector2(50, 100), new Vector2(20, 20));

        var hit = Raycast.RaycastFirst(
            world, new Ray(Vector2.Zero, new Vector2(1, 0)));

        Assert.Null(hit);
    }

    [Fact]
    public void RayBehindBox_NoHit()
    {
        var world = new World();
        CreateBox(world, new Vector2(50, 0), new Vector2(20, 20));

        // Смотрим в обратную сторону
        var hit = Raycast.RaycastFirst(
            world, new Ray(Vector2.Zero, new Vector2(-1, 0)));

        Assert.Null(hit);
    }

    [Fact]
    public void RayMaxDistance_Stops()
    {
        var world = new World();
        CreateBox(world, new Vector2(100, 0), new Vector2(20, 20));

        // Луч вправо на 50 — не дотянется до стены
        var hit = Raycast.RaycastFirst(
            world, new Ray(Vector2.Zero, new Vector2(1, 0), maxDistance: 50f));

        Assert.Null(hit);
    }

    [Fact]
    public void LayerMask_FiltersOut()
    {
        var world = new World();
        CreateBox(world, new Vector2(50, 0), new Vector2(20, 20), Layer.Enemy);

        var hit = Raycast.RaycastFirst(
            world, new Ray(Vector2.Zero, new Vector2(1, 0)),
            mask: Layer.Wall);

        Assert.Null(hit);
    }

    [Fact]
    public void RaycastAll_ReturnsSortedByDistance()
    {
        var world = new World();
        CreateBox(world, new Vector2(30, 0), new Vector2(10, 10));
        CreateBox(world, new Vector2(70, 0), new Vector2(10, 10));

        var results = new List<RaycastHit>();
        Raycast.RaycastAll(
            world, new Ray(Vector2.Zero, new Vector2(1, 0)), results);

        Assert.Equal(2, results.Count);
        Assert.True(results[0].Distance < results[1].Distance);
    }
}