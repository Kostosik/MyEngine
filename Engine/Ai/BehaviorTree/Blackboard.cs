using System.Numerics;

namespace MyEngine.Ai.BehaviorTree;

/// <summary>
/// Ключ-значение хранилище для нод дерева.
/// Типизированные хелперы для Vector2, float, Entity.
/// </summary>
public sealed class Blackboard
{
    private readonly Dictionary<string, object?> _data = new();

    public void Set<T>(string key, T value) => _data[key] = value;

    public T? Get<T>(string key)
        => _data.TryGetValue(key, out var v) && v is T t ? t : default;

    public bool TryGet<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out var v) && v is T t)
        {
            value = t;
            return true;
        }
        value = default!;
        return false;
    }

    public void Remove(string key) => _data.Remove(key);
    public void Clear() => _data.Clear();

    // Удобные сокращения
    public void SetVector(string key, Vector2 v) => Set(key, v);
    public Vector2? GetVector(string key) => Get<Vector2>(key);

    public void SetFloat(string key, float v) => Set(key, v);
    public float GetFloat(string key, float fallback = 0f)
        => Get<float?>(key) ?? fallback;
}