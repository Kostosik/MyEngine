using MyEngine.Timers;

namespace Tests;

public class SchedulerTests
{
    [Fact]
    public void After_FiresOnce()
    {
        var s = new Scheduler();
        int count = 0;
        s.After(1f, () => count++);

        s.Update(0.5f);
        Assert.Equal(0, count);

        s.Update(0.6f); // суммарно 1.1 > 1
        Assert.Equal(1, count);

        s.Update(1f);
        Assert.Equal(1, count); // больше не сработает
    }

    [Fact]
    public void Every_FiresRepeatedly()
    {
        var s = new Scheduler();
        int count = 0;
        s.Every(0.5f, () => count++);

        s.Update(0.5f);
        Assert.Equal(1, count);

        s.Update(0.5f);
        Assert.Equal(2, count);

        s.Update(1.0f);
        Assert.Equal(4, count); // ещё 2 раза
    }

    [Fact]
    public void Every_WithCount_Limits()
    {
        var s = new Scheduler();
        int count = 0;
        s.Every(0.5f, () => count++, repeatCount: 3);

        for (int i = 0; i < 10; i++) s.Update(0.5f);
        Assert.Equal(3, count);
    }

    [Fact]
    public void Tween_InterpolatesFrom0To1()
    {
        var s = new Scheduler();
        var values = new List<float>();
        s.Tween(1f, t => values.Add(t));

        s.Update(0.25f);
        s.Update(0.25f);
        s.Update(0.25f);
        s.Update(0.25f);

        Assert.Equal(4, values.Count);
        Assert.Equal(0.25f, values[0], 2);
        Assert.Equal(1f, values[3], 2);
    }

    [Fact]
    public void Cancel_StopsTask()
    {
        var s = new Scheduler();
        int count = 0;
        var h = s.After(1f, () => count++);

        s.Update(0.5f);
        s.Cancel(h);
        s.Update(1f);

        Assert.Equal(0, count);
    }

    [Fact]
    public void Pause_And_Resume()
    {
        var s = new Scheduler();
        int count = 0;
        var h = s.After(1f, () => count++);

        s.Update(0.5f);
        s.Pause(h);
        s.Update(2f);
        Assert.Equal(0, count);

        s.Resume(h);
        s.Update(0.6f);
        Assert.Equal(1, count);
    }

    [Fact]
    public void Clear_CancelsAll()
    {
        var s = new Scheduler();
        int count = 0;
        s.After(1f, () => count++);
        s.After(2f, () => count++);
        s.After(3f, () => count++);

        s.Clear();
        s.Update(5f);

        Assert.Equal(0, count);
    }
}