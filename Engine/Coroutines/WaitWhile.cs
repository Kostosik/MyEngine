namespace MyEngine.Coroutines;

/// <summary>
/// Ждать, пока условие истинно.
/// Пример: yield return new WaitWhile(() => playerIsAlive);
/// </summary>
public sealed class WaitWhile : IYieldInstruction
{
    private readonly Func<bool> _condition;

    public WaitWhile(Func<bool> condition) => _condition = condition;

    public bool IsDone => !_condition();

    public void Update(float dt) { }
    public void Reset() { }
}