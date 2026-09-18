namespace MyEngine.Coroutines;

/// <summary>
/// Ждать N кадров. Полезно, когда секунды неточны.
/// Пример: yield return new WaitForFrames(10);
/// </summary>
public sealed class WaitForFrames : IYieldInstruction
{
    private readonly int _count;
    private int _elapsed;

    public WaitForFrames(int count) => _count = count;

    public bool IsDone => _elapsed >= _count;

    public void Update(float dt) => _elapsed++;

    public void Reset() => _elapsed = 0;
}