namespace MyEngine.Ecs;

public sealed class Entity
{
    private object?[] _components = System.Array.Empty<object?>();
    public bool IsAlive = true;

    /// <summary>
    /// Уникальный ID внутри World. Присваивается при создании, не меняется.
    /// Используется для сохранений, редактора, отладки, сети.
    /// </summary>
    public int Id { get; internal set; }
    public IEnumerable<Type> ComponentTypes
    {
        get
        {
            for (int i = 0; i < _components.Length; i++)
            {
                if (_components[i] != null)
                    yield return _components[i]!.GetType();
            }
        }
    }
    public T Add<T>(T component) where T : class
    {
#if DEBUG
        MyEngine.Diagnostics.Validation.ComponentValidator.Validate(component);

        var attrs = typeof(T).GetCustomAttributes(
            typeof(RequireComponentAttribute), inherit: true)
            as RequireComponentAttribute[];

        if (attrs != null)
        {
            foreach (var attr in attrs)
            {
                foreach (var required in attr.RequiredTypes)
                {
                    int reqIdx = ComponentRegistry.GetIndex(required);
                    bool has = reqIdx < _components.Length && _components[reqIdx] != null;
                    if (!has)
                    {
                        // БЫЛО: throw
                        // СТАЛО: Warn, продолжаем
                        MyEngine.Diagnostics.Log.Error("Entity",
                            $"#{Id}: adding {typeof(T).Name} without required {required.Name}");
                    }
                }
            }
        }
#endif

        int idx = ComponentRegistry.GetIndex<T>();
        EnsureCapacity(idx + 1);
        _components[idx] = component;
        return component;
    }

    public T? Get<T>() where T : class
    {
        int idx = ComponentRegistry.GetIndex<T>();
        if (idx >= _components.Length) return null;
        return _components[idx] as T;
    }

    public bool Has<T>() where T : class
    {
        int idx = ComponentRegistry.GetIndex<T>();
        return idx < _components.Length && _components[idx] != null;
    }

    public void Remove<T>() where T : class
    {
        int idx = ComponentRegistry.GetIndex<T>();
        if (idx < _components.Length) _components[idx] = null;
    }

    private void EnsureCapacity(int needed)
    {
        if (_components.Length >= needed) return;
        int newSize = _components.Length == 0 ? 8 : _components.Length;
        while (newSize < needed) newSize *= 2;
        System.Array.Resize(ref _components, newSize);
    }

    public object? GetBoxed(Type type)
    {
        int idx = ComponentRegistry.GetIndex(type);
        if (idx >= _components.Length) return null;
        return _components[idx];
    }

    public void AddBoxed(object component)
    {
        int idx = ComponentRegistry.GetIndex(component.GetType());
        EnsureCapacity(idx + 1);
        _components[idx] = component;
    }
}