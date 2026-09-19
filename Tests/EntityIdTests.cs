using MyEngine.Ecs;

namespace Tests;

public class EntityIdTests
{
    [Fact]
    public void Entities_HaveUniqueIds()
    {
        var w = new World();
        var e1 = w.Create();
        var e2 = w.Create();
        var e3 = w.Create();

        Assert.NotEqual(e1.Id, e2.Id);
        Assert.NotEqual(e2.Id, e3.Id);
        Assert.NotEqual(e1.Id, e3.Id);
    }

    [Fact]
    public void GetById_ReturnsEntity()
    {
        var w = new World();
        var e = w.Create();
        Assert.Same(e, w.GetById(e.Id));
    }

    [Fact]
    public void GetById_Missing_ReturnsNull()
    {
        var w = new World();
        Assert.Null(w.GetById(9999));
    }

    [Fact]
    public void Destroy_RemovesFromIndex()
    {
        var w = new World();
        var e = w.Create();
        int id = e.Id;

        w.Destroy(e);

        Assert.Null(w.GetById(id));
        Assert.False(e.IsAlive);
    }

    [Fact]
    public void CreateWithId_UsesExactId()
    {
        var w = new World();
        var e = w.CreateWithId(100);

        Assert.Equal(100, e.Id);
        Assert.Same(e, w.GetById(100));
    }

    [Fact]
    public void CreateWithId_Duplicate_Throws()
    {
        var w = new World();
        w.CreateWithId(100);
        Assert.Throws<InvalidOperationException>(() => w.CreateWithId(100));
    }

    [Fact]
    public void Create_AfterCreateWithId_SkipsUsedIds()
    {
        var w = new World();
        w.CreateWithId(50);
        var next = w.Create();

        Assert.True(next.Id > 50, "Следующий ID должен быть больше использованных");
    }
}