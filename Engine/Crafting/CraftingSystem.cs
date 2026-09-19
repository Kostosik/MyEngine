using MyEngine.Ecs;
using MyEngine.Items;

namespace MyEngine.Crafting;

/// <summary>
/// Автоматически крафтит в машинах с CraftingMachine и Inventory.
/// Каждый тик:
///   1. Если крафт не идёт — проверить, можно ли начать (хватает ли входа).
///   2. Если идёт — тикать прогресс.
///   3. По завершении — вычесть вход, добавить выход в OutputBuffer.
/// </summary>
public sealed class CraftingSystem : ISystem
{
    private readonly RecipeRegistry _recipes;
    private readonly ItemRegistry _items;

    public CraftingSystem(RecipeRegistry recipes, ItemRegistry items)
    {
        _recipes = recipes;
        _items = items;
    }

    public void Update(World world, float dt)
    {
        foreach (var e in world.Query().With<CraftingMachine>().With<Inventory>())
        {
            var machine = e.Get<CraftingMachine>()!;
            var inv = e.Get<Inventory>()!;

            if (string.IsNullOrEmpty(machine.RecipeId)) continue;

            var recipe = _recipes.Get(machine.RecipeId);
            if (recipe == null) continue;

            // Продолжаем крафт
            if (machine.Progress > 0f)
            {
                machine.Progress += dt;

                if (machine.Progress >= recipe.CraftTime)
                {
                    // Завершаем
                    ConsumeInputs(inv, recipe);
                    ProduceOutputs(machine, recipe);
                    machine.Progress = 0f;
                }
                continue;
            }

            // Пробуем начать новый
            if (HasInputs(inv, recipe))
            {
                machine.Progress = 0.001f; // начало
            }
        }
    }

    private bool HasInputs(Inventory inv, Recipe recipe)
    {
        foreach (var kv in recipe.Input)
            if (!InventoryOperations.Has(inv, kv.Key, kv.Value))
                return false;
        return true;
    }

    private void ConsumeInputs(Inventory inv, Recipe recipe)
    {
        foreach (var kv in recipe.Input)
            InventoryOperations.Remove(inv, kv.Key, kv.Value);
    }

    private void ProduceOutputs(CraftingMachine machine, Recipe recipe)
    {
        foreach (var kv in recipe.Output)
            machine.OutputBuffer += kv.Value;
    }
}