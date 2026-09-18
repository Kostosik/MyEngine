using System.Diagnostics;

namespace MyEngine.Diagnostics;

/// <summary>
/// Профайлер: FPS, время кадра, время отдельных систем.
///
/// Использование:
///   profiler.BeginFrame();
///   using (profiler.Measure("Physics")) { ... }
///   profiler.EndFrame();
/// </summary>
public sealed class Profiler
{
    private struct Stat
    {
        public double LastMs;
        public double AvgMs;
        public double PeakMs;
    }

    private readonly Dictionary<string, Stat> _stats = new(32);
    private readonly Stopwatch _frameSw = new();
    private readonly float[] _frameHistory = new float[180];
    private int _frameHistoryIndex;

    private int _framesSinceReport;
    private double _reportTimer;

    private const double ReportInterval = 0.5;
    private const double AvgSmoothing = 0.9;

    /// <summary>Целевой FPS. По умолчанию 60.</summary>
    public int TargetFps { get; set; } = 60;

    /// <summary>Бюджет на кадр в миллисекундах.</summary>
    public double FrameBudgetMs => 1000.0 / TargetFps;

    /// <summary>Сколько бюджета использовано (0..1+).</summary>
    public double BudgetUsed => CurrentFrameMs / FrameBudgetMs;

    /// <summary>Скользящее среднее использования бюджета.</summary>
    public double AvgBudgetUsed { get; private set; }

    /// <summary>Сколько кадров подряд превысили бюджет.</summary>
    public int ConsecutiveOverBudget { get; private set; }

    /// <summary>Счётчик превышений за сессию.</summary>
    public int TotalOverBudgetFrames { get; private set; }

    /// <summary>Общее число кадров за сессию.</summary>
    public long TotalFrames { get; private set; }

    public int GcGen0 { get; private set; }
    public int GcGen1 { get; private set; }
    public int GcGen2 { get; private set; }

    public long AvgAllocatedPerFrame { get; private set; }
    private long _allocSum;
    private int _allocSamples;

    public long ManagedMemoryBytes { get; private set; }
    public long AllocatedPerFrameBytes { get; private set; }

    private long _lastAllocBytes;

    public double CurrentFrameMs { get; private set; }
    public double Fps { get; private set; }
    public double AvgFrameMs { get; private set; }
    public double PeakFrameMs { get; private set; }

    public float[] FrameHistory => _frameHistory;
    public int FrameHistoryIndex => _frameHistoryIndex;

    public void BeginFrame() => _frameSw.Restart();

    public void EndFrame()
    {
        _frameSw.Stop();
        CurrentFrameMs = _frameSw.Elapsed.TotalMilliseconds;
        TotalFrames++;

        // === GC counters ===
        GcGen0 = GC.CollectionCount(0);
        GcGen1 = GC.CollectionCount(1);
        GcGen2 = GC.CollectionCount(2);
        ManagedMemoryBytes = GC.GetTotalMemory(false);

        long currentAlloc = GC.GetAllocatedBytesForCurrentThread();
        long frameAlloc = currentAlloc - _lastAllocBytes;
        _lastAllocBytes = currentAlloc;
        AllocatedPerFrameBytes = frameAlloc;
        // Сколько памяти аллоцировалось за этот кадр
        AllocatedPerFrameBytes = currentAlloc - _lastAllocBytes;
        _lastAllocBytes = currentAlloc;

        _allocSum += frameAlloc;
        _allocSamples++;
        if (_allocSamples >= 60)
        {
            AvgAllocatedPerFrame = _allocSum / _allocSamples;
            _allocSum = 0;
            _allocSamples = 0;
        }

        // История кадров для графика
        _frameHistory[_frameHistoryIndex] = (float)CurrentFrameMs;
        _frameHistoryIndex = (_frameHistoryIndex + 1) % _frameHistory.Length;

        // FPS и средний кадр — раз в ReportInterval секунд
        _framesSinceReport++;
        _reportTimer += CurrentFrameMs / 1000.0;



        if (_reportTimer >= ReportInterval)
        {
            Fps = _framesSinceReport / _reportTimer;
            AvgFrameMs = _reportTimer * 1000.0 / _framesSinceReport;
            _framesSinceReport = 0;
            _reportTimer = 0;
        }

        // Frame budget
        double budgetUse = CurrentFrameMs / FrameBudgetMs;
        AvgBudgetUsed = AvgBudgetUsed * 0.99 + budgetUse * 0.01;

        if (CurrentFrameMs > FrameBudgetMs)
        {
            ConsecutiveOverBudget++;
            TotalOverBudgetFrames++;
        }
        else
        {
            ConsecutiveOverBudget = 0;
        }

        // Пик кадра — за всю сессию
        if (CurrentFrameMs > PeakFrameMs) PeakFrameMs = CurrentFrameMs;
    }

    /// <summary>Начать замер системы. Возвращаемый Scope — struct, без boxing.</summary>
    public Scope Measure(string name) => new Scope(this, name);

    /// <summary>Снимок статистики систем. Ключ — имя системы.</summary>
    public IReadOnlyDictionary<string, (double last, double avg, double peak)> Snapshot()
    {
        var result = new Dictionary<string, (double, double, double)>(_stats.Count);
        foreach (var kv in _stats)
            result[kv.Key] = (kv.Value.LastMs, kv.Value.AvgMs, kv.Value.PeakMs);
        return result;
    }

    /// <summary>Сбросить пики и статистику.</summary>
    public void ResetStats()
    {
        _stats.Clear();
        PeakFrameMs = 0;
    }

    private void AddSample(string name, double ms)
    {
        if (!_stats.TryGetValue(name, out var stat))
        {
            stat = new Stat { LastMs = ms, AvgMs = ms, PeakMs = ms };
        }
        else
        {
            stat.LastMs = ms;
            stat.AvgMs = stat.AvgMs * AvgSmoothing + ms * (1.0 - AvgSmoothing);
            if (ms > stat.PeakMs) stat.PeakMs = ms;
        }
        _stats[name] = stat;
    }

    /// <summary>
    /// Измеритель одной системы. Struct — не аллоцирует при каждом Measure.
    /// </summary>
    public readonly struct Scope : IDisposable
    {
        private readonly Profiler _profiler;
        private readonly string _name;
        private readonly long _startTicks;

        internal Scope(Profiler profiler, string name)
        {
            _profiler = profiler;
            _name = name;
            _startTicks = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            long end = Stopwatch.GetTimestamp();
            double ms = (end - _startTicks) * 1000.0 / Stopwatch.Frequency;
            _profiler.AddSample(_name, ms);
        }
    }
}