using MyEngine.Diagnostics;
using System.Collections.Concurrent;

namespace MyEngine.Threading;

public sealed class JobSystem : IDisposable
{
    private readonly Thread[] _workers;
    private readonly BlockingCollection<Action> _queue = new();
    private readonly ManualResetEventSlim _idle = new(true);
    private int _outstanding;
    private volatile bool _disposed;
    private JobProfiler? _profiler;

    public int WorkerCount => _workers.Length;

    public JobSystem(int workerCount = 0)
    {
        if (workerCount <= 0)
            workerCount = System.Math.Max(1, Environment.ProcessorCount - 1);

        _workers = new Thread[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            var t = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = $"JobWorker{i}"
            };
            t.Start();
            _workers[i] = t;
        }
    }

    public void AttachProfiler(JobProfiler profiler) => _profiler = profiler;

    public void ParallelFor(int from, int to, Action<int> body)
        => ParallelFor("ParallelFor", from, to, body);

    public void ParallelFor(string name, int from, int to, Action<int> body)
    {
        int count = to - from;
        if (count <= 0) return;

        int workers = _workers.Length;
        if (workers == 0 || count == 1)
        {
            if (_profiler != null)
            {
                _profiler.Begin(out long t0);
                try { for (int i = from; i < to; i++) body(i); }
                finally { _profiler.Record(t0, name, Environment.CurrentManagedThreadId); }
            }
            else
            {
                for (int i = from; i < to; i++) body(i);
            }
            return;
        }

        int chunkSize = (count + workers - 1) / workers;
        int chunks = (count + chunkSize - 1) / chunkSize;

        _idle.Reset();
        Interlocked.Exchange(ref _outstanding, chunks);

        var prof = _profiler;
        for (int c = 0; c < chunks; c++)
        {
            int start = from + c * chunkSize;
            int end = System.Math.Min(to, start + chunkSize);
            _queue.Add(() =>
            {
                long t0 = 0;
                if (prof != null) prof.Begin(out t0);

                try
                {
                    for (int i = start; i < end; i++) body(i);
                }
                finally
                {
                    prof?.Record(t0, name, Environment.CurrentManagedThreadId);
                }
            });
        }

        _idle.Wait();
    }

    public void Enqueue(Action job) => _queue.Add(job);

    private void WorkerLoop()
    {
        foreach (var job in _queue.GetConsumingEnumerable())
        {
            try { job(); }
            catch (Exception ex) { Log.Error("[JobSystem] Job threw:", ex.Message); }

            if (Interlocked.Decrement(ref _outstanding) == 0)
                _idle.Set();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _queue.CompleteAdding();
        foreach (var t in _workers) t.Join(500);
        _queue.Dispose();
        _idle.Dispose();
    }
}