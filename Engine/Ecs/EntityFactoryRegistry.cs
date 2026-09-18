using System.Numerics;

namespace MyEngine.Ecs;

/// <summary>
/// Реестр фабрик сущностей по имени. Позволяет создавать сущности
/// по имени: "slime", "wall", "conveyor".
///
/// Не привязан к данным — функции пишет игра. Используется для
/// консоли, редактора, тестов, спавна по имени.
///
/// Пример:
///   registry.Register("slime", (w, pos) => EntityFactory.CreateSlime(w, pos));
///   var e = registry.Spawn(world, "slime", new Vector2(100, 100));
/// </summary>
public sealed class EntityFactoryRegistry
{
    private readonly Dictionary<string, Func<World, Vector2, Entity>> _factories = new();

    public void Register(string name, Func<World, Vector2, Entity> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be empty");
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _factories[name] = factory;
    }

    /// <summary>Создать сущность по имени. Кидает, если имени нет.</summary>
    public Entity Spawn(World world, string name, Vector2 position)
    {
        if (!_factories.TryGetValue(name, out var f))
            throw new InvalidOperationException(
                $"Unknown entity type: '{name}'. Known: {string.Join(", ", _factories.Keys)}");
        return f(world, position);
    }

    /// <summary>Попробовать создать. Возвращает null, если имени нет.</summary>
    public Entity? TrySpawn(World world, string name, Vector2 position)
    {
        if (!_factories.TryGetValue(name, out var f)) return null;
        return f(world, position);
    }

    public bool Has(string name) => _factories.ContainsKey(name);
    public IEnumerable<string> Names => _factories.Keys;
    public int Count => _factories.Count;
    public void Clear() => _factories.Clear();
}