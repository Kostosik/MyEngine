using MyEngine.Ecs;

namespace MyEngine.Tests;

public class RequireComponentTests
{
    private class A { }
    private class B { }

    [RequireComponent(typeof(A))]
    private class NeedsA { }

    [Fact]
    public void RequireComponent_PassesWhenPresent()
    {
        var e = new Entity();
        e.Add(new A());
        e.Add(new NeedsA());   // должен пройти
    }

    [Fact]
    public void RequireComponent_ThrowsWhenMissing()
    {
        var e = new Entity();
        Assert.Throws<InvalidOperationException>(() => e.Add(new NeedsA()));
    }
}