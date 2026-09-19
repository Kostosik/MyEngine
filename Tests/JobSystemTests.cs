using MyEngine.Threading;

namespace Tests;

public class JobSystemTests
{
    [Fact]
    public void ParallelFor_ExecutesEveryIndex()
    {
        using var jobs = new JobSystem(3);
        int n = 1000;
        var flags = new int[n];

        jobs.ParallelFor(0, n, i => flags[i] = i + 1);

        for (int i = 0; i < n; i++)
            Assert.Equal(i + 1, flags[i]);
    }

    [Fact]
    public void ParallelFor_EmptyRange_DoesNothing()
    {
        using var jobs = new JobSystem(2);
        int calls = 0;
        jobs.ParallelFor(5, 5, _ => Interlocked.Increment(ref calls));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void ParallelFor_SingleItem_Runs()
    {
        using var jobs = new JobSystem(4);
        int value = 0;
        jobs.ParallelFor(0, 1, _ => value = 42);
        Assert.Equal(42, value);
    }

    [Fact]
    public void ParallelFor_UsesMultipleThreads()
    {
        using var jobs = new JobSystem(3);
        var threads = new System.Collections.Concurrent.ConcurrentBag<int>();

        jobs.ParallelFor(0, 300, _ =>
        {
            threads.Add(Environment.CurrentManagedThreadId);
            Thread.Sleep(1);
        });

        // Ожидаем минимум 2 разных потока (иногда 1 если задач мало)
        Assert.True(threads.Distinct().Count() >= 2,
            $"Used {threads.Distinct().Count()} thread(s)");
    }

    [Fact]
    public void JobGraph_RunsInOrder()
    {
        using var jobs = new JobSystem(3);
        var graph = new JobGraph(jobs);

        var order = new System.Collections.Concurrent.ConcurrentQueue<string>();

        var a = graph.Add(() => { Thread.Sleep(20); order.Enqueue("A"); });
        var b = graph.Add(() => { Thread.Sleep(20); order.Enqueue("B"); });
        var c = graph.Add(() => order.Enqueue("C"), a, b);
        var d = graph.Add(() => order.Enqueue("D"), c);

        graph.WaitAll();

        var result = order.ToArray();
        // A и B — в любом порядке, C после обоих, D после C
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Equal("C", result[2]);
        Assert.Equal("D", result[3]);
    }
}