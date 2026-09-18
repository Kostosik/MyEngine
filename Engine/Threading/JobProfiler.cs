using System.Diagnostics;

namespace MyEngine.Threading;

public struct JobEvent
{
    public long StartTicks;
    public long EndTicks;
    public int WorkerId;
    public string Name;
}

public sealed class JobProfiler
{
    private const int BufferSize = 4096;
    private readonly JobEvent[] _events = new JobEvent[BufferSize];
    private int _head;

    public int MainThreadId { get; }

    public JobProfiler(int mainThreadId)
    {
        MainThreadId = mainThreadId;
    }

    public void Begin(out long ticks) => ticks = Stopwatch.GetTimestamp();

    public void Record(long startTicks, string name, int workerId)
    {
        long end = Stopwatch.GetTimestamp();
        var ev = new JobEvent
        {
            StartTicks = startTicks,
            EndTicks = end,
            WorkerId = workerId,
            Name = name
        };
        int idx = Interlocked.Increment(ref _head) - 1;
        _events[idx & (BufferSize - 1)] = ev;
    }

    /// <summary>Копирует последние события в переданный буфер, возвращает count.</summary>
    public int Snapshot(JobEvent[] target)
    {
        int head = Volatile.Read(ref _head);
        int n = System.Math.Min(head, System.Math.Min(BufferSize, target.Length));
        for (int i = 0; i < n; i++)
        {
            int idx = (head - n + i) & (BufferSize - 1);
            target[i] = _events[idx];
        }
        return n;
    }
}