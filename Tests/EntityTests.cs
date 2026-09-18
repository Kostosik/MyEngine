using MyEngine.Ecs;

namespace MyEngine.Tests;

public class EntityTests
{
    private class Health { public int Hp; }
    private class Position { public float X; }

    [Fact]
    public void Add_And_Get()
    {
        var e = new Entity();
        e.Add(new Health { Hp = 10 });
        Assert.Equal(10, e.Get<Health>()!.Hp);
    }

    [Fact]
    public void Get_Missing_ReturnsNull()
    {
        var e = new Entity();
        Assert.Null(e.Get<Health>());
    }

    [Fact]
    public void Has_ReflectsPresence()
    {
        var e = new Entity();
        Assert.False(e.Has<Health>());
        e.Add(new Health());
        Assert.True(e.Has<Health>());
        e.Remove<Health>();
        Assert.False(e.Has<Health>());
    }

    [Fact]
    public void ManyTypes_Work()
    {
        var e = new Entity();
        e.Add(new Health { Hp = 1 });
        e.Add(new Position { X = 2 });
        Assert.Equal(1, e.Get<Health>()!.Hp);
        Assert.Equal(2, e.Get<Position>()!.X);
    }
}