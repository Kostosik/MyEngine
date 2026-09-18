namespace MyEngine.Pooling;

/// <summary>
/// Реестр пулов. Позволяет достать пул по типу.
/// Не статический — живёт на уровне игры (в Application или GameContext).
/// </summary>
public sealed class PoolManager
{
    private readonly Dictionary<Type, object> _pools = new();

    /// <summary>
    /// Зарегистрировать пул. Возвращает его же — для цепочки вызовов.
    /// </summary>
    public Pool<T> Register<T>(Pool<T> pool) where T : class
    {
        _pools[typeof(T)] = pool;
        return pool;
    }

    /// <summary>
    /// Создать и зарегистрировать пул за один вызов.
    /// </summary>
    public Pool<T> Create<T>(
        Func<T> factory,
        int initialSize = 0,
        int maxSize = -1,
        Action<T>? onRent = null,
        Action<T>? onReturn = null) where T : class
    {
        var pool = new Pool<T>(factory, initialSize, maxSize, onRent, onReturn);
        _pools[typeof(T)] = pool;
        return pool;
    }

    /// <summary>
    /// Достать пул по типу. Кидает исключение, если не зарегистрирован.
    /// </summary>
    public Pool<T> Get<T>() where T : class
    {
        if (_pools.TryGetValue(typeof(T), out var p))
            return (Pool<T>)p;
        throw new InvalidOperationException($"Pool not registered: {typeof(T).Name}");
    }

    /// <summary>Попробовать достать — вернёт null, если нет.</summary>
    public Pool<T>? TryGet<T>() where T : class
        => _pools.TryGetValue(typeof(T), out var p) ? (Pool<T>)p : null;

    public bool Has<T>() where T : class
        => _pools.ContainsKey(typeof(T));

    public void Clear()
    {
        foreach (var p in _pools.Values)
        {
            var method = p.GetType().GetMethod("Clear");
            method?.Invoke(p, null);
        }
        _pools.Clear();
    }
}