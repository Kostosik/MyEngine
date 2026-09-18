namespace MyEngine.Utility;

/// <summary>
/// Детерминированный генератор случайных чисел.
///
/// Использует xorshift128+ — быстрый, качественный, воспроизводимый.
/// Одинаковый seed → одинаковая последовательность на любой машине,
/// в любом запуске, в любой .NET-версии.
///
/// Для обычных игр — не нужен, используй System.Random.
/// Для реплеев, мультиплеера, тестов — обязателен.
/// </summary>
public struct DeterministicRandom
{
    private ulong _s0;
    private ulong _s1;

    public DeterministicRandom(int seed)
    {
        // Простое инициализирование из одного seed
        ulong s = (ulong)(uint)seed;
        _s0 = SplitMix64(ref s);
        _s1 = SplitMix64(ref s);
    }

    public DeterministicRandom(ulong seed0, ulong seed1)
    {
        _s0 = seed0 == 0 ? 0x9E3779B97F4A7C15UL : seed0;
        _s1 = seed1 == 0 ? 0xBF58476D1CE4E5B9UL : seed1;
    }

    /// <summary>Следующее 64-битное число.</summary>
    public ulong NextULong()
    {
        ulong x = _s0;
        ulong y = _s1;
        _s0 = y;
        x ^= x << 23;
        _s1 = x ^ y ^ (x >> 17) ^ (y >> 26);
        return _s1 + y;
    }

    /// <summary>Следующее 32-битное беззнаковое.</summary>
    public uint NextUInt() => (uint)(NextULong() >> 32);

    /// <summary>Следующее int в диапазоне [0, max).</summary>
    public int NextInt(int max)
    {
        if (max <= 0) return 0;
        uint r = NextUInt();
        return (int)(r % (uint)max);
    }

    /// <summary>Следующее int в диапазоне [min, max).</summary>
    public int NextInt(int min, int max)
    {
        if (max <= min) return min;
        return min + NextInt(max - min);
    }

    /// <summary>Float в диапазоне [0, 1).</summary>
    public float NextFloat()
    {
        // 24 бита мантиссы — точность float
        uint r = NextUInt() >> 8;
        return r / (float)(1 << 24);
    }

    /// <summary>Float в диапазоне [min, max).</summary>
    public float NextFloat(float min, float max)
        => min + NextFloat() * (max - min);

    /// <summary>Double в [0, 1).</summary>
    public double NextDouble()
    {
        ulong r = NextULong() >> 11;  // 53 бита
        return r / (double)(1UL << 53);
    }

    /// <summary>Bool с вероятностью p.</summary>
    public bool NextBool(float p = 0.5f) => NextFloat() < p;

    /// <summary>Случайный элемент из массива.</summary>
    public T NextItem<T>(IReadOnlyList<T> items)
    {
        if (items.Count == 0) throw new ArgumentException("Empty list");
        return items[NextInt(items.Count)];
    }

    /// <summary>Перемешать массив на месте (Fisher-Yates).</summary>
    public void Shuffle<T>(IList<T> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = NextInt(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    /// <summary>Сохранить состояние (для сохранений/сети).</summary>
    public (ulong s0, ulong s1) SaveState() => (_s0, _s1);

    /// <summary>Восстановить состояние.</summary>
    public void LoadState(ulong s0, ulong s1)
    {
        _s0 = s0;
        _s1 = s1;
    }

    // === SplitMix64 для инициализации ===
    private static ulong SplitMix64(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}