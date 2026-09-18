namespace MyEngine.Items;

/// <summary>
/// Определение типа предмета. Одно на все экземпляры.
/// Регистрируется в ItemRegistry при старте игры.
/// </summary>
public sealed class Item
{
    /// <summary>Уникальный идентификатор. "iron_ore", "copper_plate".</summary>
    public string Id = "";

    /// <summary>Отображаемое имя.</summary>
    public string Name = "";

    /// <summary>Максимум в одном стеке. 1 — не стек, 100 — обычно.</summary>
    public int MaxStack = 100;

    /// <summary>
    /// Категория для группировки: "resource", "intermediate", "building".
    /// Свободная строка — движок сам не интерпретирует.
    /// </summary>
    public string Category = "";

    /// <summary>Ключ текстуры для UI (не Texture2D, чтобы не грузить сейчас).</summary>
    public string IconKey = "";

    /// <summary>Объект для расширения — игра хранит что угодно.</summary>
    public object? UserData;
}