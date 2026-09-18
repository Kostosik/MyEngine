using MyEngine.Diagnostics;

namespace MyEngine.Items;

/// <summary>
/// Реестр всех типов предметов. Игра регистрирует свои предметы
/// при старте. Движок сам не знает ничего про конкретные предметы.
/// </summary>
public sealed class ItemRegistry
{
    private readonly Dictionary<string, Item> _items = new();

    public int Count => _items.Count;
    public IEnumerable<Item> All => _items.Values;

    /// <summary>Зарегистрировать предмет.</summary>
    public void Register(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
            throw new ArgumentException("Item.Id must not be empty");

        if (_items.ContainsKey(item.Id))
        {
            Log.Warn("ItemRegistry", $"Item '{item.Id}' already registered, overwriting");
        }

        _items[item.Id] = item;
    }

    /// <summary>Достать предмет по ID. Null, если нет.</summary>
    public Item? Get(string id)
        => _items.GetValueOrDefault(id);

    /// <summary>Есть ли предмет.</summary>
    public bool Has(string id) => _items.ContainsKey(id);

    /// <summary>Получить MaxStack или 100 по умолчанию, если предмет не найден.</summary>
    public int GetMaxStack(string id)
        => _items.TryGetValue(id, out var item) ? item.MaxStack : 100;

    /// <summary>Очистить реестр. При рестарте.</summary>
    public void Clear() => _items.Clear();
}