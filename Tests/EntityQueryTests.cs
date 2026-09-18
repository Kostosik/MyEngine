using MyEngine.Ecs;

namespace MyEngine.Tests;

public class EntityQueryTests
{
    private class A { }
    private class B { }
    private class C { }
    private class D { }

    private static World MakeWorld()
    {
        var w = new World();
        return w;
    }

    // ============================================================
    // With — один тип
    // ============================================================

    [Fact]
    public void With_FiltersByComponent()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A());
        var e2 = w.Create(); e2.Add(new B());

        var result = w.Query().With<A>().ToList();

        Assert.Single(result);
        Assert.Same(e1, result[0]);
    }

    [Fact]
    public void With_Chain_TwoComponents()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A()); e1.Add(new B());
        var e2 = w.Create(); e2.Add(new A());
        var e3 = w.Create(); e3.Add(new B());

        var result = w.Query().With<A>().With<B>().ToList();

        Assert.Single(result);
        Assert.Same(e1, result[0]);
    }

    [Fact]
    public void With_Chain_ThreeComponents()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A()); e1.Add(new B()); e1.Add(new C());
        var e2 = w.Create(); e2.Add(new A()); e2.Add(new B());

        var result = w.Query().With<A>().With<B>().With<C>().ToList();

        Assert.Single(result);
        Assert.Same(e1, result[0]);
    }

    // ============================================================
    // Without
    // ============================================================

    [Fact]
    public void Without_ExcludesComponent()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A()); e1.Add(new B());
        var e2 = w.Create(); e2.Add(new A());

        var result = w.Query().With<A>().Without<B>().ToList();

        Assert.Single(result);
        Assert.Same(e2, result[0]);
    }

    [Fact]
    public void Without_TwoExcludes()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A()); e1.Add(new B());
        var e2 = w.Create(); e2.Add(new A()); e2.Add(new C());
        var e3 = w.Create(); e3.Add(new A());

        var result = w.Query().With<A>().Without<B>().Without<C>().ToList();

        Assert.Single(result);
        Assert.Same(e3, result[0]);
    }

    // ============================================================
    // WithAny
    // ============================================================

    [Fact]
    public void WithAny_MatchesEither()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A());
        var e2 = w.Create(); e2.Add(new B());
        var e3 = w.Create(); e3.Add(new C());

        var result = w.Query().WithAny<A, B>().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(e1, result);
        Assert.Contains(e2, result);
    }

    [Fact]
    public void WithAny_ThreeTypes()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A());
        var e2 = w.Create(); e2.Add(new B());
        var e3 = w.Create(); e3.Add(new C());
        var e4 = w.Create(); e4.Add(new D());

        var result = w.Query().WithAny<A, B, C>().ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(e1, result);
        Assert.Contains(e2, result);
        Assert.Contains(e3, result);
        Assert.DoesNotContain(e4, result);
    }

    // ============================================================
    // Мёртвые сущности
    // ============================================================

    [Fact]
    public void Query_IgnoresDeadEntities()
    {
        var w = MakeWorld();
        var e1 = w.Create(); e1.Add(new A());
        var e2 = w.Create(); e2.Add(new A());

        w.Destroy(e1);

        var result = w.Query().With<A>().ToList();

        Assert.Single(result);
        Assert.Same(e2, result[0]);
    }

    [Fact]
    public void Query_Empty_NoEntities()
    {
        var w = MakeWorld();
        var result = w.Query().With<A>().ToList();
        Assert.Empty(result);
    }

    // ============================================================
    // Ленивость
    // ============================================================

    [Fact]
    public void Query_IsLazy_NoAllocation()
    {
        var w = MakeWorld();
        for (int i = 0; i < 100; i++)
        {
            var e = w.Create();
            e.Add(new A());
        }

        // Берём только первые 5
        int count = 0;
        foreach (var e in w.Query().With<A>())
        {
            count++;
            if (count >= 5) break;
        }

        Assert.Equal(5, count);
    }

    // ============================================================
    // Старый API работает
    // ============================================================

    [Fact]
    public void OldWithApi_StillWorks()
    {
        var w = MakeWorld();
        var e = w.Create(); e.Add(new A());

        Assert.Same(e, w.With<A>().First());
        Assert.Same(e, w.FirstWith<A>());
    }
}