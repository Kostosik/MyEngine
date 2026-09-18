namespace MyEngine.Time;

/// <summary>
/// Управление течением времени в игре.
///
/// Даёт:
///   - DeltaTime (с учётом TimeScale) для Update
///   - UnscaledDeltaTime для UI, камеры, звука — того, что не должно замедляться
///   - Pause / Resume
///   - TimeScale (slow-mo, fast-forward, hit-stop)
///   - Счётчик кадров и общее время
///
/// Scheduler, физика, системы — все должны использовать DeltaTime.
/// ImGui, камера shake, звук — UnscaledDeltaTime.
/// </summary>
public static class TimeEngine
{
    /// <summary>Множитель времени. 1 = норма, 0 = пауза, 0.5 = slow-mo, 2 = ускорение.</summary>
    public static float TimeScale = 1f;

    /// <summary>Если true — DeltaTime возвращает 0, всё стоит. Не путать с TimeScale = 0.</summary>
    public static bool Paused = false;

    /// <summary>Реальный dt (не масштабированный). Для UI, камеры, звука.</summary>
    public static float UnscaledDeltaTime { get; private set; }

    /// <summary>Масштабированный dt. Для игровой логики.</summary>
    public static float DeltaTime => Paused ? 0f : UnscaledDeltaTime * TimeScale;

    /// <summary>Реальное время с начала игры (секунды). Не масштабируется.</summary>
    public static double UnscaledTotalTime { get; private set; }

    /// <summary>Игровое время (сумма всех DeltaTime). Масштабируется.</summary>
    public static double TotalTime { get; private set; }

    /// <summary>Номер текущего кадра.</summary>
    public static long Frame { get; private set; }

    // Внутренний hit-stop таймер
    private static float _hitStopTimer;
    private static float _savedTimeScale = 1f;

    /// <summary>Целевой FPS. 60 по умолчанию. Меняется Application'ом.</summary>
    public static int TargetFps { get; set; } = 60;

    /// <summary>Фиксированный шаг симуляции в секундах.</summary>
    public static float FixedDeltaTime => 1f / TargetFps;

    /// <summary>Сколько секунд реального времени прошло с последнего кадра.</summary>
    public static float RealDeltaTime => UnscaledDeltaTime;

    /// <summary>Отношение DeltaTime / UnscaledDeltaTime. 1 при обычной игре, 0 при паузе.</summary>
    public static float EffectiveScale => UnscaledDeltaTime > 0.0001f
        ? DeltaTime / UnscaledDeltaTime
        : 0f;

    /// <summary>
    /// Отформатированное время сессии. "01:23:45" или "12:34" для коротких сессий.
    /// </summary>
    public static string FormatSession()
    {
        var t = System.TimeSpan.FromSeconds(UnscaledTotalTime);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes:D2}:{t.Seconds:D2}";
    }
    /// <summary>
    /// Обновление раз в кадр. Вызывается из Application.
    /// </summary>
    public static void Update(float realDeltaTime)
    {
        UnscaledDeltaTime = realDeltaTime;
        UnscaledTotalTime += realDeltaTime;
        TotalTime += DeltaTime;
        Frame++;

        // Обработка hit-stop
        if (_hitStopTimer > 0f)
        {
            _hitStopTimer -= realDeltaTime;
            if (_hitStopTimer <= 0f)
            {
                // Восстанавливаем time scale
                TimeScale = _savedTimeScale;
                _hitStopTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Замирание на N секунд. Применяется для эффекта удара:
    /// всё останавливается на 0.05 сек, потом продолжается.
    /// </summary>
    public static void HitStop(float duration)
    {
        if (_hitStopTimer > 0f) return; // уже идёт
        _savedTimeScale = TimeScale;
        TimeScale = 0f;
        _hitStopTimer = duration;
    }

    /// <summary>Сбросить всё. Используется при рестарте игры.</summary>
    public static void Reset()
    {
        TimeScale = 1f;
        Paused = false;
        _hitStopTimer = 0f;
        _savedTimeScale = 1f;
        // TotalTime, Frame — не сбрасываем, они не мешают
    }
}