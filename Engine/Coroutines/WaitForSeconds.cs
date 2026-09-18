namespace MyEngine.Coroutines;

/// <summary>
/// Ждать N секунд игрового времени (с учётом TimeScale).
/// Пример: yield return new WaitForSeconds(2f);
/// </summary>
public sealed class WaitForSeconds : IYieldInstruction
{
    private readonly float _duration;
    private float _elapsed;

    public WaitForSeconds(float duration) => _duration = duration;

    public bool IsDone => _elapsed >= _duration;

    public void Update(float dt) => _elapsed += dt;

    public void Reset() => _elapsed = 0f;
}