namespace MyEngine.Diagnostics;

/// <summary>
/// Реестр значений для наблюдения в WatchWindow.
///
/// Пример:
///   Watch.Add("HP", () => player.Get&lt;Health&gt;()?.Hp ?? 0);
///   Watch.Add("Enemies", () => world.With&lt;EnemyTag&gt;().Count());
///   Watch.AddSection("Performance");
///   Watch.Add("FPS", () => Profiler.Fps);
///
/// Значения вычисляются ТОЛЬКО в момент отрисовки окна.
/// Если окно закрыто — лямбды не вызываются, CPU не тратится.
/// </summary>
public static class Watch
{
    /// <summary>Одна запись — имя, лямбда, и последнее значение для отрисовки.</summary>
    public sealed class Entry
    {
        public string Name = "";
        public Func<string>? Getter;
        public Func<object?>? RawGetter;
        public bool IsSection;
    }

    private static readonly List<Entry> _entries = new();
    private static readonly object _lock = new();

    /// <summary>Все записи. Используется WatchWindow.</summary>
    public static IReadOnlyList<Entry> Entries
    {
        get { lock (_lock) return _entries.ToArray(); }
    }

    // ============================================================
    // Добавление
    // ============================================================

    /// <summary>Добавить значение с форматированием через лямбду.</summary>
    public static void Add(string name, Func<string> getter)
    {
        lock (_lock)
        {
            _entries.Add(new Entry
            {
                Name = name,
                Getter = getter
            });
        }
    }

    /// <summary>Добавить числовое значение — красиво отформатируется.</summary>
    public static void Add(string name, Func<int> getter)
        => Add(name, () => getter().ToString());

    public static void Add(string name, Func<float> getter)
        => Add(name, () => getter().ToString("F2"));

    public static void Add(string name, Func<double> getter)
        => Add(name, () => getter().ToString("F3"));

    public static void Add(string name, Func<bool> getter)
        => Add(name, () => getter() ? "TRUE" : "FALSE");

    /// <summary>Добавить значение с автоматическим форматированием любого типа.</summary>
    public static void AddValue<T>(string name, Func<T> getter)
        => Add(name, () => getter()?.ToString() ?? "(null)");

    /// <summary>Добавить заголовок-разделитель.</summary>
    public static void AddSection(string title)
    {
        lock (_lock)
        {
            _entries.Add(new Entry
            {
                Name = title,
                IsSection = true
            });
        }
    }

    // ============================================================
    // Удаление
    // ============================================================

    /// <summary>Удалить запись по имени.</summary>
    public static void Remove(string name)
    {
        lock (_lock)
            _entries.RemoveAll(e => e.Name == name);
    }

    /// <summary>Очистить всё. Вызывать при рестарте игры.</summary>
    public static void Clear()
    {
        lock (_lock)
            _entries.Clear();
    }
}