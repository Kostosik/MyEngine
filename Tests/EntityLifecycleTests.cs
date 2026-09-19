using MyEngine.Ecs;
using MyEngine.Events;

namespace Tests;

public class EntityLifecycleTests
{
    [Fact]
    public void Create_PublishesCreatedEvent()
    {
        var bus = new EventBus();
        var w = new World();
        w.AttachEvents(bus);

        int received = 0;
        Entity? captured = null;
        bus.Subscribe<EntityCreatedEvent>(evt =>
        {
            received++;
            captured = evt.Entity;
        });

        var e = w.Create();

        Assert.Equal(1, received);
        Assert.Same(e, captured);
    }

    [Fact]
    public void Destroy_PublishesDestroyedEvent()
    {
        var bus = new EventBus();
        var w = new World();
        w.AttachEvents(bus);

        var e = w.Create();

        int received = 0;
        Entity? captured = null;
        bus.Subscribe<EntityDestroyedEvent>(evt =>
        {
            received++;
            captured = evt.Entity;
        });

        w.Destroy(e);

        Assert.Equal(1, received);
        Assert.Same(e, captured);
    }

    [Fact]
    public void DestroyTwice_PublishesOnce()
    {
        var bus = new EventBus();
        var w = new World();
        w.AttachEvents(bus);

        var e = w.Create();

        int received = 0;
        bus.Subscribe<EntityDestroyedEvent>(evt => received++);

        w.Destroy(e);
        w.Destroy(e);  // повторный вызов — не должен публиковать

        Assert.Equal(1, received);
    }

    [Fact]
    public void NoEventBus_NoEvents()
    {
        var w = new World();   // AttachEvents не вызывается

        // Не должно упасть
        var e = w.Create();
        w.Destroy(e);

        Assert.False(e.IsAlive);
    }

    [Fact]
    public void MultipleEntities_AllEvents()
    {
        var bus = new EventBus();
        var w = new World();
        w.AttachEvents(bus);

        int created = 0;
        int destroyed = 0;
        bus.Subscribe<EntityCreatedEvent>(_ => created++);
        bus.Subscribe<EntityDestroyedEvent>(_ => destroyed++);

        for (int i = 0; i < 10; i++) w.Create();

        var all = w.Entities.ToList();
        foreach (var e in all) w.Destroy(e);

        Assert.Equal(10, created);
        Assert.Equal(10, destroyed);
    }

    [Fact]
    public void DestroyedEntity_StillReadableInHandler()
    {
        var bus = new EventBus();
        var w = new World();
        w.AttachEvents(bus);

        var e = w.Create();
        e.Add(new TestComp { Value = 42 });

        int captured = 0;
        bus.Subscribe<EntityDestroyedEvent>(evt =>
        {
            var comp = evt.Entity.Get<TestComp>();
            captured = comp?.Value ?? -1;
        });

        w.Destroy(e);

        Assert.Equal(42, captured);  // компонент доступен в обработчике
    }

    private class TestComp { public int Value; }
}