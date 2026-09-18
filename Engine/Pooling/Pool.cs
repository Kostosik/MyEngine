namespace MyEngine.Pooling;

/// <summary>
/// Универсальный пул объектов.
///
/// Идея: объекты не создаются и не удаляются каждый раз,
/// а берутся из пула и возвращаются обратно.
/// Это убирает аллокации и работу GC.
///
/// Использование:
///   var pool = new Pool&lt;Bullet&gt;(() => new Bullet(), initialSize: 32);
///   var bullet = pool.Rent();
///   bullet.Position = ...;
///   pool.Return(bullet);
///
/// Пул не потокобезопасен — если нужен из нескольких потоков,
/// оборачивай в lock или используй пул на поток.
/// </summary>
public sealed class Pool<T> where T : class
{
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly Stack<T> _available;
    private readonly int _maxSize;

    private int _rentedCount;
    private int _createdCount;

    /// <summary>
    /// Создать пул.
    /// factory — как создавать новый объект, когда пул пуст.
    /// initialSize — сколько создать заранее.
    /// maxSize — максимум объектов в пуле. Если вернуть больше, лишние удаляются (GC). -1 = без лимита.
    /// onRent — что делать при выдаче (например, Reset()).
    /// onReturn — что делать при возврате (например, снять флаги).
    /// </summary>
    public Pool(
        Func<T> factory,
        int initialSize = 0,
        int maxSize = -1,
        Action<T>? onRent = null,
        Action<T>? onReturn = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _maxSize = maxSize;
        _onRent = onRent;
        _onReturn = onReturn;
        _available = new Stack<T>(initialSize > 0 ? initialSize : 16);

        for (int i = 0; i < initialSize; i++)
        {
            _available.Push(_factory());
            _createdCount++;
        }
    }

    /// <summary>Сколько объектов сейчас свободно в пуле.</summary>
    public int AvailableCount => _available.Count;

    /// <summary>Сколько объектов выдано и не возвращено.</summary>
    public int RentedCount => _rentedCount;

    /// <summary>Сколько всего объектов создано за всю жизнь пула.</summary>
    public int CreatedCount => _createdCount;

    /// <summary>Сколько объектов в пуле всего (свободные + арендованные).</summary>
    public int TotalCount => _available.Count + _rentedCount;

    /// <summary>
    /// Взять объект из пула. Если пул пуст — создаётся новый.
    /// </summary>
    public T Rent()
    {
        T obj = _available.Count > 0 ? _available.Pop() : CreateNew();
        _rentedCount++;
        _onRent?.Invoke(obj);
        return obj;
    }

    /// <summary>
    /// Вернуть объект в пул. Объект не удаляется — будет переиспользован.
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null) return;

        _onReturn?.Invoke(obj);
        _rentedCount--;

        // Если превышен максимум — не возвращаем в пул, отдаём GC
        if (_maxSize > 0 && _available.Count >= _maxSize)
            return;

        _available.Push(obj);
    }

    /// <summary>
    /// Очистить пул — удалить все свободные объекты.
    /// Арендованные не трогаются (они у кого-то в руках).
    /// </summary>
    public void Clear() => _available.Clear();

    /// <summary>
    /// Предзаполнить пул. Полезно, если знаешь,
    /// что в ближайшее время понадобится много объектов.
    /// </summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _available.Push(_factory());
            _createdCount++;
        }
    }

    private T CreateNew()
    {
        _createdCount++;
        return _factory();
    }
}