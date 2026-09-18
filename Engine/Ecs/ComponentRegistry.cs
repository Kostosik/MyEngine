using System.Collections.Concurrent;

namespace MyEngine.Ecs;

internal static class ComponentRegistry
{
    private static readonly ConcurrentDictionary<Type, int> _indices = new();
    private static int _next;

    public static int GetIndex<T>() where T : class => GetIndex(typeof(T));

    public static int GetIndex(Type type)
    {
        return _indices.GetOrAdd(type, _ => Interlocked.Increment(ref _next) - 1);
    }

    public static int Count => _next;
}