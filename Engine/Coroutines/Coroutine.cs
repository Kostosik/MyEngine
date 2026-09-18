using System.Collections;

namespace MyEngine.Coroutines;

/// <summary>
/// Идентификатор запущенной корутины. Позволяет её остановить.
/// </summary>
public readonly struct CoroutineHandle
{
    internal readonly int Id;
    internal CoroutineHandle(int id) => Id = id;
    public bool IsValid => Id > 0;
}

/// <summary>
/// Одна запущенная корутина. Держит IEnumerator, двигает его,
/// обновляет текущий yield instruction.
/// </summary>
internal sealed class Coroutine
{
    public int Id;
    public IEnumerator Enumerator;
    public IYieldInstruction? CurrentYield;
    public bool IsDone;
    public bool Cancelled;

    private readonly Stack<IEnumerator> _stack = new();

    public Coroutine(int id, IEnumerator enumerator)
    {
        Id = id;
        Enumerator = enumerator;
        _stack.Push(enumerator);
    }

    /// <summary>
    /// Тик корутины. Возвращает true, если она ещё активна.
    /// </summary>
    public bool Tick(float dt)
    {
        if (Cancelled || IsDone) return false;

        // Сначала обновляем текущее ожидание
        if (CurrentYield != null)
        {
            CurrentYield.Update(dt);
            if (!CurrentYield.IsDone) return true;
            CurrentYield = null;
        }

        // Двигаем корутину вперёд
        while (CurrentYield == null)
        {
            var top = _stack.Peek();
            bool moved = top.MoveNext();

            // Вложенная корутина закончилась
            if (!moved)
            {
                _stack.Pop();
                if (_stack.Count == 0)
                {
                    IsDone = true;
                    return false;
                }
                // Возвращаемся к родительской — сразу продолжаем
                continue;
            }

            var y = top.Current;

            // Обычный yield instruction
            if (y is IYieldInstruction yi)
            {
                CurrentYield = yi;
                return true;
            }

            // Вложенная корутина (IEnumerator)
            if (y is IEnumerator nested)
            {
                _stack.Push(nested);
                continue;
            }

            // null или что-то непонятное — просто пропускаем
            if (y == null) continue;

            // Неизвестный тип — считаем, что это мгновенное ожидание
            continue;
        }

        return true;
    }
}