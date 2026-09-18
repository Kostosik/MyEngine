namespace MyEngine.Ticking;

/// <summary>
/// Частота обновления. Значение — сколько тиков между обновлениями.
///
/// Конвейеры — EveryTick (1). Печи — Every4 (4, 15 Гц при 60 FPS).
/// Большие машины — Every16 (3.75 Гц). Радары — Every64.
///
/// Степени двойки — удобно: tick % rate == 0.
/// </summary>
public enum TickRate
{
    /// <summary>Каждый тик — 60 Гц при базовом 60.</summary>
    EveryTick = 1,

    /// <summary>Каждый 2-й тик — 30 Гц.</summary>
    Every2 = 2,

    /// <summary>Каждый 4-й тик — 15 Гц.</summary>
    Every4 = 4,

    /// <summary>Каждый 8-й тик — 7.5 Гц.</summary>
    Every8 = 8,

    /// <summary>Каждый 16-й тик — 3.75 Гц.</summary>
    Every16 = 16,

    /// <summary>Каждый 32-й тик — ~2 Гц.</summary>
    Every32 = 32,

    /// <summary>Каждый 64-й тик — ~1 Гц.</summary>
    Every64 = 64,
}