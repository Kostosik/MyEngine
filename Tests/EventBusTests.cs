using MyEngine.Events;

namespace Tests;

public class EventBusTests
{
    private class TestEvent { public int Value; }

    [Fact]
    public void Subscribe_And_Publish()
    {
        var bus = new EventBus();
        int value = 0;
        bus.Subscribe<TestEvent>(e => value = e.Value);
        bus.Publish(new TestEvent { Value = 42 });
        Assert.Equal(42, value);
    }

    [Fact]
    public void Priority_HigherFirst()
    {
        var bus = new EventBus();
        var order = new List<string>();
        bus.Subscribe<TestEvent>(_ => order.Add("low"), priority: 0);
        bus.Subscribe<TestEvent>(_ => order.Add("high"), priority: 10);
        bus.Subscribe<TestEvent>(_ => order.Add("mid"), priority: 5);

        bus.Publish(new TestEvent());
        Assert.Equal(new[] { "high", "mid", "low" }, order);
    }

    [Fact]
    public void SubscribeOnce_FiresOnce()
    {
        var bus = new EventBus();
        int count = 0;
        bus.SubscribeOnce<TestEvent>(_ => count++);
        bus.Publish(new TestEvent());
        bus.Publish(new TestEvent());
        Assert.Equal(1, count);
    }

    [Fact]
    public void Dispose_Unsubscribes()
    {
        var bus = new EventBus();
        int count = 0;
        var sub = bus.Subscribe<TestEvent>(_ => count++);
        bus.Publish(new TestEvent());
        sub.Dispose();
        bus.Publish(new TestEvent());
        Assert.Equal(1, count);
    }

    [Fact]
    public void Handler_Exception_DoesNotBreakOthers()
    {
        var bus = new EventBus();
        int called = 0;
        bus.Subscribe<TestEvent>(_ => throw new Exception("boom"), priority: 10);
        bus.Subscribe<TestEvent>(_ => called++);

        bus.Publish(new TestEvent());
        Assert.Equal(1, called);
    }

    [Fact]
    public void Unsubscribe_Removes()
    {
        var bus = new EventBus();
        int count = 0;
        Action<TestEvent> h = _ => count++;
        bus.Subscribe(h);
        bus.Publish(new TestEvent());
        bus.Unsubscribe(h);
        bus.Publish(new TestEvent());
        Assert.Equal(1, count);
    }
}