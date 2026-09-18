using System.Collections;

namespace MyEngine.Coroutines;

/// <summary>
/// Менеджер корутин. Хранит все активные, обновляет их каждый кадр,
/// запускает новые, останавливает по handle.
/// </summary>
public sealed class CoroutineRunner
{
    private readonly List<Coroutine> _active = new();
    private readonly List<Coroutine> _pending = new();
    private int _nextId = 1;

    public int ActiveCount => _active.Count;

    /// <summary>
    /// Запустить корутину. Возвращает handle для отмены.
    /// Новая корутина начнёт тикать со следующего Update.
    /// </summary>
    public CoroutineHandle Start(IEnumerator enumerator)
    {
        var c = new Coroutine(_nextId++, enumerator);
        _pending.Add(c);
        return new CoroutineHandle(c.Id);
    }

    /// <summary>Остановить корутину. Она не выполнит оставшиеся шаги.</summary>
    public void Stop(CoroutineHandle handle)
    {
        foreach (var c in _active)
            if (c.Id == handle.Id) { c.Cancelled = true; return; }

        foreach (var c in _pending)
            if (c.Id == handle.Id) { c.Cancelled = true; return; }
    }

    /// <summary>Остановить все корутины. Используется при рестарте игры.</summary>
    public void StopAll()
    {
        foreach (var c in _active) c.Cancelled = true;
        foreach (var c in _pending) c.Cancelled = true;
    }

    /// <summary>
    /// Обновить все активные корутины. Вызывать раз в кадр.
    /// dt — уже scaled (Time.DeltaTime).
    /// </summary>
    public void Update(float dt)
    {
        // Принимаем новые
        if (_pending.Count > 0)
        {
            _active.AddRange(_pending);
            _pending.Clear();
        }

        // Тикаем и чистим завершённые
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var c = _active[i];
            bool alive = c.Tick(dt);
            if (!alive)
                _active.RemoveAt(i);
        }
    }
}