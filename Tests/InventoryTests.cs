using MyEngine.Items;

namespace MyEngine.Tests;

public class InventoryTests
{
    private static ItemRegistry MakeRegistry()
    {
        var r = new ItemRegistry();
        r.Register(new Item { Id = "iron_ore", Name = "Iron Ore", MaxStack = 100 });
        r.Register(new Item { Id = "copper_ore", Name = "Copper Ore", MaxStack = 100 });
        r.Register(new Item { Id = "sword", Name = "Sword", MaxStack = 1 });
        return r;
    }

    // ============================================================
    // ItemStack
    // ============================================================

    [Fact]
    public void ItemStack_Empty()
    {
        var s = ItemStack.Empty;
        Assert.True(s.IsEmpty);
        Assert.Equal(0, s.Count);
    }

    [Fact]
    public void ItemStack_Not_Empty()
    {
        var s = new ItemStack("iron_ore", 5);
        Assert.True(s.IsNotEmpty);
        Assert.Equal(5, s.Count);
        Assert.Equal("iron_ore", s.ItemId);
    }

    // ============================================================
    // Inventory
    // ============================================================

    [Fact]
    public void Inventory_NewIsEmpty()
    {
        var inv = new Inventory(10);
        Assert.True(inv.IsEmpty());
        Assert.Equal(0, inv.UsedSlots());
        Assert.Equal(0, inv.TotalItems());
    }

    // ============================================================
    // Add
    // ============================================================

    [Fact]
    public void Add_SingleStack()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);

        int added = InventoryOperations.Add(inv, r, "iron_ore", 50);

        Assert.Equal(50, added);
        Assert.Equal(50, InventoryOperations.Count(inv, "iron_ore"));
    }

    [Fact]
    public void Add_FillsExistingStackFirst()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);

        InventoryOperations.Add(inv, r, "iron_ore", 50);
        InventoryOperations.Add(inv, r, "iron_ore", 30);

        // Должен быть один слот с 80
        Assert.Equal(1, inv.UsedSlots());
        Assert.Equal(80, InventoryOperations.Count(inv, "iron_ore"));
    }

    [Fact]
    public void Add_OverflowToNewSlot()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);

        // MaxStack = 100, добавляем 150 → 1 слот 100, 1 слот 50
        int added = InventoryOperations.Add(inv, r, "iron_ore", 150);

        Assert.Equal(150, added);
        Assert.Equal(2, inv.UsedSlots());
        Assert.Equal(150, InventoryOperations.Count(inv, "iron_ore"));
    }

    [Fact]
    public void Add_NoSpace_ReturnsPartial()
    {
        var r = MakeRegistry();
        var inv = new Inventory(1);   // только 1 слот
        r.Register(new Item { Id = "copper_ore", MaxStack = 100 });

        // MaxStack = 100, но место только в 1 слоте
        int added = InventoryOperations.Add(inv, r, "iron_ore", 250);

        Assert.Equal(100, added);   // добавилось ровно в один слот
    }

    [Fact]
    public void Add_UnknownItem_DoesNothing()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);

        int added = InventoryOperations.Add(inv, r, "unobtainium", 50);

        Assert.Equal(0, added);
        Assert.True(inv.IsEmpty());
    }

    // ============================================================
    // Remove
    // ============================================================

    [Fact]
    public void Remove_Partial()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 80);

        int removed = InventoryOperations.Remove(inv, "iron_ore", 30);

        Assert.Equal(30, removed);
        Assert.Equal(50, InventoryOperations.Count(inv, "iron_ore"));
    }

    [Fact]
    public void Remove_All()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 50);

        int removed = InventoryOperations.Remove(inv, "iron_ore", 50);

        Assert.Equal(50, removed);
        Assert.True(inv.IsEmpty());
    }

    [Fact]
    public void Remove_MoreThanHave_RemovesAll()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 30);

        int removed = InventoryOperations.Remove(inv, "iron_ore", 100);

        Assert.Equal(30, removed);
        Assert.True(inv.IsEmpty());
    }

    [Fact]
    public void Remove_AcrossMultipleSlots()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 150); // 100 + 50 в двух слотах

        int removed = InventoryOperations.Remove(inv, "iron_ore", 120);

        Assert.Equal(120, removed);
        Assert.Equal(30, InventoryOperations.Count(inv, "iron_ore"));
        Assert.Equal(1, inv.UsedSlots());
    }

    // ============================================================
    // FindSlot
    // ============================================================

    [Fact]
    public void FindSlot_FindsItem()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 50);
        InventoryOperations.Add(inv, r, "copper_ore", 30);

        int ironSlot = InventoryOperations.FindSlot(inv, "iron_ore");
        int copperSlot = InventoryOperations.FindSlot(inv, "copper_ore");
        int missingSlot = InventoryOperations.FindSlot(inv, "sword");

        Assert.Equal(0, ironSlot);
        Assert.Equal(1, copperSlot);
        Assert.Equal(-1, missingSlot);
    }

    // ============================================================
    // Transfer
    // ============================================================

    [Fact]
    public void Transfer_AllItems()
    {
        var r = MakeRegistry();
        var from = new Inventory(10);
        var to = new Inventory(10);
        InventoryOperations.Add(from, r, "iron_ore", 50);

        int moved = InventoryOperations.Transfer(from, to, r, "iron_ore", 50);

        Assert.Equal(50, moved);
        Assert.True(from.IsEmpty());
        Assert.Equal(50, InventoryOperations.Count(to, "iron_ore"));
    }

    [Fact]
    public void Transfer_PartialDueToFullTarget()
    {
        var r = MakeRegistry();
        var from = new Inventory(10);
        var to = new Inventory(1);
        InventoryOperations.Add(from, r, "iron_ore", 150);

        // В to 1 слот × MaxStack 100 = влезет 100
        int moved = InventoryOperations.Transfer(from, to, r, "iron_ore", 150);

        Assert.Equal(100, moved);
        Assert.Equal(50, InventoryOperations.Count(from, "iron_ore"));
        Assert.Equal(100, InventoryOperations.Count(to, "iron_ore"));
    }

    // ============================================================
    // TransferStack
    // ============================================================

    [Fact]
    public void TransferStack_MovesEntireSlot()
    {
        var r = MakeRegistry();
        var from = new Inventory(10);
        var to = new Inventory(10);
        InventoryOperations.Add(from, r, "iron_ore", 42);

        bool moved = InventoryOperations.TransferStack(from, 0, to);

        Assert.True(moved);
        Assert.True(from.IsEmpty());
        Assert.Equal(42, InventoryOperations.Count(to, "iron_ore"));
    }

    [Fact]
    public void TransferStack_NoSpace_ReturnsFalse()
    {
        var r = MakeRegistry();
        var from = new Inventory(10);
        var to = new Inventory(0 + 1);
        // Забьём to полностью
        InventoryOperations.Add(to, r, "copper_ore", 100);

        InventoryOperations.Add(from, r, "iron_ore", 50);

        bool moved = InventoryOperations.TransferStack(from, 0, to);

        Assert.False(moved);
        Assert.Equal(50, InventoryOperations.Count(from, "iron_ore"));
    }

    // ============================================================
    // SplitHalf
    // ============================================================

    [Fact]
    public void SplitHalf_DividesStack()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 50);

        var half = InventoryOperations.SplitHalf(inv, 0);

        Assert.Equal(25, half.Count);
        Assert.Equal("iron_ore", half.ItemId);
        Assert.Equal(25, InventoryOperations.Count(inv, "iron_ore"));
    }

    [Fact]
    public void SplitHalf_OddCount_KeepsExtraInSource()
    {
        var r = MakeRegistry();
        var inv = new Inventory(10);
        InventoryOperations.Add(inv, r, "iron_ore", 5);

        var half = InventoryOperations.SplitHalf(inv, 0);

        // 5/2 = 2, оставшийся 3 в источнике
        Assert.Equal(2, half.Count);
        Assert.Equal(3, InventoryOperations.Count(inv, "iron_ore"));
    }
}