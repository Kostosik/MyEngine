using MyEngine.Ecs;

namespace MyEngine.Tests;

public class WorldSingletonTests
{
    private class TestSettings { public int Value; }

    [Fact]
    public void SetAndGet_Works()
    {
        var w = new World();
        w.SetSingleton(new TestSettings { Value = 42 });
        Assert.Equal(42, w.GetSingleton<TestSettings>()!.Value);
    }

    [Fact]
    public void Get_Missing_ReturnsNull()
    {
        var w = new World();
        Assert.Null(w.GetSingleton<TestSettings>());
    }

    [Fact]
    public void GetOrCreate_CreatesOnce()
    {
        var w = new World();
        int calls = 0;
        var a = w.GetOrCreateSingleton(() => { calls++; return new TestSettings(); });
        var b = w.GetOrCreateSingleton(() => { calls++; return new TestSettings(); });
        Assert.Same(a, b);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Remove_Clears()
    {
        var w = new World();
        w.SetSingleton(new TestSettings());
        w.RemoveSingleton<TestSettings>();
        Assert.Null(w.GetSingleton<TestSettings>());
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var w = new World();
        w.SetSingleton(new TestSettings());
        w.ClearSingletons();
        Assert.Null(w.GetSingleton<TestSettings>());
    }
}