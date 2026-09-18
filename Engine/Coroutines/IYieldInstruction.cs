namespace MyEngine.Coroutines;

/// <summary>
/// Что-то, что можно yield-нуть из корутины.
/// Пока IsDone() возвращает false — корутина стоит.
/// </summary>
public interface IYieldInstruction
{
    /// <summary>Обновить состояние ожидания. Вызывается каждый кадр.</summary>
    void Update(float dt);

    /// <summary>Завершено ли ожидание. Если true — корутина продолжит выполнение.</summary>
    bool IsDone { get; }

    /// <summary>Сбросить состояние. Нужен при повторном использовании.</summary>
    void Reset();
}