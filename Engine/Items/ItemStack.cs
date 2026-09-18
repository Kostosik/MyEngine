namespace MyEngine.Items;

/// <summary>
/// Один стек предметов в слоте.
/// struct — не аллоцирует, можно хранить в массивах.
///
/// Пустой стек: ItemId == "" и Count == 0.
/// </summary>
public struct ItemStack : IEquatable<ItemStack>
{
    /// <summary>ID предмета. "" — слот пуст.</summary>
    public string ItemId;

    /// <summary>Количество. 0 — слот пуст.</summary>
    public int Count;

    public readonly bool IsEmpty => Count <= 0 || string.IsNullOrEmpty(ItemId);
    public readonly bool IsNotEmpty => !IsEmpty;

    public ItemStack(string itemId, int count)
    {
        ItemId = itemId ?? "";
        Count = count < 0 ? 0 : count;
    }

    public static ItemStack Empty => new("", 0);

    public readonly bool Equals(ItemStack other)
        => ItemId == other.ItemId && Count == other.Count;

    public override readonly bool Equals(object? obj)
        => obj is ItemStack s && Equals(s);

    public override readonly int GetHashCode()
        => HashCode.Combine(ItemId, Count);

    public override readonly string ToString()
        => IsEmpty ? "(empty)" : $"{ItemId} x{Count}";
}