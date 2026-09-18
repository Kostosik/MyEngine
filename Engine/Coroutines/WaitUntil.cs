namespace MyEngine.Coroutines;

/// <summary>
/// Ждать, пока условие не станет true.
/// Пример: yield return new WaitUntil(() => playerHp &lt;= 0);
/// </summary>
public sealed class WaitUntil : IYieldInstruction
{
    private readonly Func<bool> _condition;

    public WaitUntil(Func<bool> condition) => _condition = condition;

    public bool IsDone => _condition();

    public void Update(float dt) { }
    public void Reset() { }
}