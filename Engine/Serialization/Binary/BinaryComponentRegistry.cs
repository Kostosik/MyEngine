using MyEngine.Diagnostics;

namespace MyEngine.Serialization.Binary;

/// <summary>
/// Реестр компонентов, умеющих бинарную сериализацию.
/// Используется BinaryWorldSerializer.
///
/// Каждый компонент регистрируется под именем (обычно имя типа).
/// На запись — имя + размер + данные.
/// На чтение — имя → фабрика → Read.
/// </summary>
public sealed class BinaryComponentRegistry
{
    private struct Entry
    {
        public Func<IBinarySerializable> Factory;
        public int Id;
    }

    private readonly Dictionary<string, Entry> _byName = new();
    private readonly Dictionary<Type, int> _byType = new();
    private readonly List<string> _namesById = new();

    /// <summary>Зарегистрировать компонент. Имя должно быть уникальным.</summary>
    public void Register<T>(Func<T> factory) where T : class, IBinarySerializable
    {
        var type = typeof(T);
        if (_byType.ContainsKey(type)) return;   // уже зарегистрирован

        int id = _namesById.Count;
        string name = type.Name;

        _byName[name] = new Entry { Factory = () => factory(), Id = id };
        _byType[type] = id;
        _namesById.Add(name);
    }

    /// <summary>Получить id типа по экземпляру. -1, если не зарегистрирован.</summary>
    public int GetId(object component)
        => _byType.GetValueOrDefault(component.GetType(), -1);

    /// <summary>Получить имя типа по id.</summary>
    public string GetName(int id)
        => id >= 0 && id < _namesById.Count ? _namesById[id] : "?";

    /// <summary>Получить фабрику по id. Null, если id неизвестен.</summary>
    public Func<IBinarySerializable>? GetFactory(int id)
        => id >= 0 && id < _namesById.Count && _byName.TryGetValue(_namesById[id], out var e)
            ? e.Factory
            : null;

    public int Count => _namesById.Count;
    public IEnumerable<string> Names => _namesById;

    public void Clear()
    {
        _byName.Clear();
        _byType.Clear();
        _namesById.Clear();
    }
}