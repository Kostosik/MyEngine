using MyEngine.Events;

namespace MyEngine.Ecs;

/// <summary>
/// Игровой мир: контейнер сущностей и глобальных сервисов.
/// Владеет реестром по ID — каждая сущность имеет уникальный
/// номер, не меняющийся за всю жизнь.
/// </summary>
public sealed class World
{
    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Type, object> _singletons = new();

    private EventBus? _events;
    // Реестр по ID. Даёт доступ к сущности по номеру — для сохранений,
    // редактора, отладки, сети. Чистится при Destroy и Cleanup.
    private readonly Dictionary<int, Entity> _byId = new();
    private int _nextId = 1;

    public IReadOnlyList<Entity> Entities => _entities;

    /// <summary>
    /// Подключить EventBus для публикации EntityCreated/Destroyed.
    /// Если не подключён — события не публикуются. Игра может
    /// вызывать в StartNewGame: world.AttachEvents(Events).
    /// </summary>
    public void AttachEvents(EventBus bus) => _events = bus;

    public Entity Create()
    {
        var e = new Entity { Id = _nextId++ };
        _entities.Add(e);
        _byId[e.Id] = e;

        _events?.Publish(new EntityCreatedEvent { Entity = e });
        return e;
    }

    public void Destroy(Entity e)
    {
        if (!e.IsAlive) return;   // уже мёртвая — не публикуем повторно
        e.IsAlive = false;
        _byId.Remove(e.Id);

        _events?.Publish(new EntityDestroyedEvent { Entity = e });
    }

    /// <summary>
    /// Все живые сущности. Для fluent-запросов:
    ///   world.Query().With&lt;Transform&gt;().Without&lt;PlayerTag&gt;()
    /// </summary>
    public IEnumerable<Entity> Query()
    {
        foreach (var e in _entities)
            if (e.IsAlive) yield return e;
    }

    /// <summary>Найти сущность по ID. Null, если нет или уже мертва.</summary>
    public Entity? GetById(int id)
        => _byId.TryGetValue(id, out var e) && e.IsAlive ? e : null;

    /// <summary>
    /// Создать сущность с конкретным ID. Для десериализации,
    /// загрузки сохранений, сетевой синхронизации.
    /// Если ID уже занят — исключение.
    /// </summary>
    public Entity CreateWithId(int id)
    {
        if (_byId.ContainsKey(id))
            throw new InvalidOperationException($"Entity with id {id} already exists");

        var e = new Entity { Id = id };
        _entities.Add(e);
        _byId[id] = e;
        if (id >= _nextId) _nextId = id + 1;
        return e;
    }

    public IEnumerable<Entity> With<T>() where T : class
    {
        foreach (var e in _entities)
            if (e.IsAlive && e.Has<T>()) yield return e;
    }

    public Entity? FirstWith<T>() where T : class
    {
        foreach (var e in _entities)
            if (e.IsAlive && e.Has<T>()) return e;
        return null;
    }

    /// <summary>
    /// Удалить мёртвые сущности из списка и реестра. Периодически
    /// вызывай, чтобы не копить мусор.
    /// </summary>
    public void Cleanup()
    {
        _entities.RemoveAll(e =>
        {
            if (e.IsAlive) return false;
            _byId.Remove(e.Id);
            return true;
        });
    }

    // ============================================================
    // Singleton-компоненты
    // ============================================================

    /// <summary>
    /// Зарегистрировать singleton-компонент. Синглтон существует
    /// вне сущностей — он один на весь мир (настройки, GameState,
    /// глобальные сервисы).
    ///
    /// Пример:
    ///   world.SetSingleton(new PhysicsSettings { ... });
    ///   var s = world.GetSingleton&lt;PhysicsSettings&gt;();
    /// </summary>
    public void SetSingleton<T>(T value) where T : class
    {
        _singletons[typeof(T)] = value;
    }

    /// <summary>
    /// Получить singleton-компонент. Возвращает null, если не задан.
    /// </summary>
    public T? GetSingleton<T>() where T : class
        => _singletons.TryGetValue(typeof(T), out var v) ? (T)v : null;

    /// <summary>
    /// Получить singleton или создать через фабрику. Удобно для
    /// ленивой инициализации настроек по умолчанию.
    /// </summary>
    public T GetOrCreateSingleton<T>(Func<T> factory) where T : class
    {
        if (_singletons.TryGetValue(typeof(T), out var v)) return (T)v;
        var created = factory();
        _singletons[typeof(T)] = created;
        return created;
    }

    /// <summary>Удалить singleton.</summary>
    public void RemoveSingleton<T>() where T : class
        => _singletons.Remove(typeof(T));

    /// <summary>Очистить все синглтоны. Вызывать при рестарте мира.</summary>
    public void ClearSingletons() => _singletons.Clear();
}