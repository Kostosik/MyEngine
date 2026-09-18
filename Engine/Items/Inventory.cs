namespace MyEngine.Items;

/// <summary>
/// Контейнер слотов. У игрока, сундука, машины — свой Inventory.
///
/// Слоты — фиксированного размера, массив ItemStack.
/// Все операции — через InventoryOperations (Add, Remove, Transfer).
/// Inventory сам ничего не знает о предметах — только массив стеков.
/// </summary>
public sealed class Inventory
{
    public ItemStack[] Slots { get; }
    public int Size => Slots.Length;

    public Inventory(int size)
    {
        if (size <= 0) throw new ArgumentException("Inventory size must be > 0");
        Slots = new ItemStack[size];
        for (int i = 0; i < size; i++)
            Slots[i] = ItemStack.Empty;
    }

    public ItemStack this[int index]
    {
        get => Slots[index];
        set => Slots[index] = value;
    }

    /// <summary>Пустой ли весь инвентарь.</summary>
    public bool IsEmpty()
    {
        foreach (var s in Slots)
            if (s.IsNotEmpty) return false;
        return true;
    }

    /// <summary>Сколько слотов занято.</summary>
    public int UsedSlots()
    {
        int n = 0;
        foreach (var s in Slots)
            if (s.IsNotEmpty) n++;
        return n;
    }

    /// <summary>Сколько всего предметов (сумма Count).</summary>
    public int TotalItems()
    {
        int n = 0;
        foreach (var s in Slots)
            n += s.Count;
        return n;
    }

    /// <summary>Очистить все слоты.</summary>
    public void Clear()
    {
        for (int i = 0; i < Slots.Length; i++)
            Slots[i] = ItemStack.Empty;
    }
}