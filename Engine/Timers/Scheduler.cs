using System.Numerics;

namespace MyEngine.Timers;

/// <summary>
/// Идентификатор запланированной задачи. Позволяет отменить её
/// или перевести на паузу.
/// </summary>
public readonly struct ScheduleHandle
{
    internal readonly int Id;
    internal ScheduleHandle(int id) => Id = id;
    public bool IsValid => Id > 0;
}

/// <summary>
/// Планировщик отложенных и повторяющихся задач.
///
/// Использует один список задач и тикает их раз в кадр (в Update,
/// переменный шаг). Не путать с фиксированным апдейтом — это сервис
/// для игрового кода, а не для физики.
///
/// Пример:
///   scheduler.After(2f, () => OpenDoor());
///   scheduler.Every(0.5f, () => SpawnEnemy(), repeatCount: 10);
///   scheduler.Tween(0.3f, t => sprite.Color.W = 1 - t);
///   var h = scheduler.After(5f, () => Explode());
///   scheduler.Cancel(h); // отменили взрыв
/// </summary>
public sealed class Scheduler
{
    private sealed class Task
    {
        public float RepeatInterval;
        public int Id;
        public Action<float>? OnTick;   // аргумент — прогресс 0..1 для твинов
        public Action? OnComplete;
        public float Duration;
        public float Elapsed;
        public float Delay;
        public bool Repeating;
        public int RemainingRepeats;    // -1 = бесконечно
        public bool Paused;
        public bool Cancelled;
    }

    private readonly List<Task> _tasks = new();
    private int _nextId = 1;

    // Пул свободных id, чтобы не расти бесконечно при повторениях
    private readonly Stack<int> _freeIds = new();

    public int ActiveCount => _tasks.Count;

    // ===================== Публичный API =====================

    /// <summary>
    /// Выполнить действие один раз через delay секунд.
    /// </summary>
    public ScheduleHandle After(float delay, Action action)
    {
        var t = Rent();
        t.Delay = delay;
        t.Duration = 0f;
        t.OnComplete = action;
        t.Repeating = false;
        _tasks.Add(t);
        return new ScheduleHandle(t.Id);
    }

    /// <summary>
    /// Выполнять действие каждые interval секунд.
    /// Первый вызов — через interval от текущего момента.
    /// repeatCount = -1 → бесконечно.
    /// </summary>
    public ScheduleHandle Every(float interval, Action action, int repeatCount = -1)
    {

        var t = Rent();
        t.Delay = interval;
        t.Duration = 0f;
        t.OnComplete = action;
        t.Repeating = true;
        t.RemainingRepeats = repeatCount;
        t.RepeatInterval = interval;
        _tasks.Add(t);
        return new ScheduleHandle(t.Id);
    }

    /// <summary>
    /// Интерполировать значение от 0 до 1 за duration секунд.
    /// onTick вызывается каждый кадр с текущим прогрессом.
    /// </summary>
    public ScheduleHandle Tween(float duration, Action<float> onTick, Action? onComplete = null)
    {
        var t = Rent();
        t.Delay = 0f;
        t.Duration = duration;
        t.OnTick = onTick;
        t.OnComplete = onComplete;
        t.Repeating = false;
        _tasks.Add(t);
        return new ScheduleHandle(t.Id);
    }

    /// <summary>
    /// Отменить задачу. Если задача уже выполнилась — ничего не делает.
    /// </summary>
    public void Cancel(ScheduleHandle handle)
    {
        foreach (var t in _tasks)
        {
            if (t.Id == handle.Id)
            {
                t.Cancelled = true;
                return;
            }
        }
    }

    /// <summary>Поставить задачу на паузу.</summary>
    public void Pause(ScheduleHandle handle)
    {
        foreach (var t in _tasks)
            if (t.Id == handle.Id) { t.Paused = true; return; }
    }

    /// <summary>Снять с паузы.</summary>
    public void Resume(ScheduleHandle handle)
    {
        foreach (var t in _tasks)
            if (t.Id == handle.Id) { t.Paused = false; return; }
    }

    /// <summary>Отменить все задачи. Использовать при рестарте уровня.</summary>
    public void Clear()
    {
        foreach (var t in _tasks) t.Cancelled = true;
    }

    // ===================== Тик =====================

    /// <summary>
    /// Обновить все задачи. Вызывать раз в кадр с переменным dt.
    /// </summary>
    public void Update(float dt)
    {
        for (int i = _tasks.Count - 1; i >= 0; i--)
        {
            var t = _tasks[i];

            if (t.Cancelled)
            {
                ReturnId(t.Id);
                _tasks.RemoveAt(i);
                continue;
            }
            if (t.Paused) continue;

            // Сколько времени доступно для этой задачи в этом кадре
            float time = dt;

            // === Задержка перед первым срабатыванием ===
            if (t.Delay > 0f)
            {
                if (t.Delay > time)
                {
                    t.Delay -= time;
                    continue; // ещё не время
                }
                time -= t.Delay; // остаток после задержки
                t.Delay = 0f;
            }

            // === Твин (Duration > 0) ===
            if (t.Duration > 0f)
            {
                t.Elapsed += time;
                float p = System.Math.Min(1f, t.Elapsed / t.Duration);
                t.OnTick?.Invoke(p);

                if (t.Elapsed >= t.Duration)
                {
                    t.OnComplete?.Invoke();
                    ReturnId(t.Id);
                    _tasks.RemoveAt(i);
                }
                continue;
            }

            // === Повторяющееся действие ===
            if (t.Repeating)
            {
                bool remove = false;

                while (true)
                {
                    t.OnComplete?.Invoke();

                    if (t.RemainingRepeats > 0)
                    {
                        t.RemainingRepeats--;
                        if (t.RemainingRepeats == 0)
                        {
                            remove = true;
                            break;
                        }
                    }

                    // Защита от бесконечного цикла при интервале 0
                    if (t.RepeatInterval <= 0f)
                    {
                        break;
                    }

                    // Уместится ли ещё один интервал?
                    if (t.RepeatInterval > time)
                    {
                        // Не уместится — запоминаем остаток
                        t.Delay = t.RepeatInterval - time;
                        break;
                    }

                    time -= t.RepeatInterval;
                }

                if (remove)
                {
                    ReturnId(t.Id);
                    _tasks.RemoveAt(i);
                }
                continue;
            }

            // === Одноразовое действие ===
            t.OnComplete?.Invoke();
            ReturnId(t.Id);
            _tasks.RemoveAt(i);
        }
    }

    // ===================== Служебное =====================

    private Task Rent()
    {
        var t = new Task();
        t.Id = _freeIds.Count > 0 ? _freeIds.Pop() : _nextId++;
        t.RemainingRepeats = 0;
        return t;
    }

    private void ReturnId(int id)
    {
        _freeIds.Push(id);
    }
}