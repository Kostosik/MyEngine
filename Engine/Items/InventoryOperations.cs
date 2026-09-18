namespace MyEngine.Items;

/// <summary>
/// Операции над инвентарями: добавление, удаление, передача.
///
/// Все методы — статические, не зависят от Entity/ECS.
/// Знают про ItemRegistry (для MaxStack) — передаётся параметром.
/// </summary>
public static class InventoryOperations
{
    /// <summary>
    /// Попытаться добавить amount предметов itemId.
    /// Возвращает сколько добавлено (может быть меньше, если не влезло).
    ///
    /// Алгоритм:
    ///   1. Дополняем существующие стеки до MaxStack.
    ///   2. Если осталось — используем пустые слоты.
    /// </summary>
    public static int Add(Inventory inv, ItemRegistry registry, string itemId, int amount)
    {
        if (amount <= 0) return 0;
        if (!registry.Has(itemId)) return 0;

        int maxStack = registry.GetMaxStack(itemId);
        int added = 0;

        // 1. Дополняем существующие стеки
        for (int i = 0; i < inv.Size && added < amount; i++)
        {
            ref var slot = ref inv.Slots[i];
            if (slot.IsEmpty || slot.ItemId != itemId) continue;
            if (slot.Count >= maxStack) continue;

            int space = maxStack - slot.Count;
            int toAdd = System.Math.Min(space, amount - added);
            slot.Count += toAdd;
            added += toAdd;
        }

        // 2. Занимаем пустые слоты
        for (int i = 0; i < inv.Size && added < amount; i++)
        {
            ref var slot = ref inv.Slots[i];
            if (slot.IsNotEmpty) continue;

            int toAdd = System.Math.Min(maxStack, amount - added);
            inv.Slots[i] = new ItemStack(itemId, toAdd);
            added += toAdd;
        }

        return added;
    }

    /// <summary>Добавить стек как есть. Возвращает остаток (0 если всё влезло).</summary>
    public static int AddStack(Inventory inv, ItemRegistry registry, ItemStack stack)
    {
        if (stack.IsEmpty) return 0;
        int added = Add(inv, registry, stack.ItemId, stack.Count);
        return stack.Count - added;
    }

    /// <summary>Сколько всего itemId в инвентаре.</summary>
    public static int Count(Inventory inv, string itemId)
    {
        int n = 0;
        foreach (var s in inv.Slots)
            if (s.ItemId == itemId) n += s.Count;
        return n;
    }

    /// <summary>Есть ли в инвентаре нужное количество.</summary>
    public static bool Has(Inventory inv, string itemId, int amount)
        => Count(inv, itemId) >= amount;

    /// <summary>
    /// Удалить amount предметов itemId.
    /// Возвращает сколько реально удалено (может быть меньше, если не хватило).
    /// </summary>
    public static int Remove(Inventory inv, string itemId, int amount)
    {
        if (amount <= 0) return 0;
        int removed = 0;

        for (int i = 0; i < inv.Size && removed < amount; i++)
        {
            ref var slot = ref inv.Slots[i];
            if (slot.IsEmpty || slot.ItemId != itemId) continue;

            int toRemove = System.Math.Min(slot.Count, amount - removed);
            slot.Count -= toRemove;
            removed += toRemove;

            if (slot.Count == 0)
                slot = ItemStack.Empty;
        }

        return removed;
    }

    /// <summary>Найти слот по itemId. -1 если нет.</summary>
    public static int FindSlot(Inventory inv, string itemId)
    {
        for (int i = 0; i < inv.Size; i++)
            if (inv.Slots[i].ItemId == itemId && inv.Slots[i].IsNotEmpty) return i;
        return -1;
    }

    /// <summary>Найти первый пустой слот. -1 если нет.</summary>
    public static int FindEmptySlot(Inventory inv)
    {
        for (int i = 0; i < inv.Size; i++)
            if (inv.Slots[i].IsEmpty) return i;
        return -1;
    }

    /// <summary>
    /// Переместить amount предметов из одного инвентаря в другой.
    /// Возвращает сколько перемещено.
    /// </summary>
    public static int Transfer(
        Inventory from, Inventory to, ItemRegistry registry,
        string itemId, int amount)
    {
        if (amount <= 0) return 0;
        if (!Has(from, itemId, amount)) return 0;

        int moved = 0;
        int maxStack = registry.GetMaxStack(itemId);

        // Идём по слотам from, отправляем в to
        for (int i = 0; i < from.Size && moved < amount; i++)
        {
            ref var srcSlot = ref from.Slots[i];
            if (srcSlot.IsEmpty || srcSlot.ItemId != itemId) continue;

            int toSend = System.Math.Min(srcSlot.Count, amount - moved);
            int accepted = Add(to, registry, itemId, toSend);

            if (accepted == 0) break;   // в to больше не влезает

            srcSlot.Count -= accepted;
            if (srcSlot.Count == 0)
                srcSlot = ItemStack.Empty;

            moved += accepted;

            if (accepted < toSend) break;   // to заполнился, дальше смысла нет
        }

        return moved;
    }

    /// <summary>Переместить один слот целиком.</summary>
    public static bool TransferStack(Inventory from, int fromSlot, Inventory to)
    {
        if (fromSlot < 0 || fromSlot >= from.Size) return false;
        var stack = from[fromSlot];
        if (stack.IsEmpty) return false;

        // Ищем пустой слот в to
        int empty = FindEmptySlot(to);
        if (empty < 0) return false;

        to[empty] = stack;
        from[fromSlot] = ItemStack.Empty;
        return true;
    }

    /// <summary>
    /// Разделить стек: взять половину из слота.
    /// Используется при ПКМ в UI — «взять половину».
    /// </summary>
    public static ItemStack SplitHalf(Inventory inv, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inv.Size) return ItemStack.Empty;
        ref var slot = ref inv.Slots[slotIndex];
        if (slot.IsEmpty) return ItemStack.Empty;

        int half = slot.Count / 2;
        if (half == 0) half = 1;

        var result = new ItemStack(slot.ItemId, half);
        slot.Count -= half;
        if (slot.Count == 0) slot = ItemStack.Empty;

        return result;
    }
}