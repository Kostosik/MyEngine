namespace MyEngine.Crafting;

/// <summary>
/// Машина, которая крафтит по рецепту.
/// Внутри — Inventory для входа и выхода.
/// </summary>
public sealed class CraftingMachine
{
    /// <summary>Id текущего рецепта.</summary>
    public string RecipeId = "";

    /// <summary>Прогресс текущего крафта (0..CraftTime).</summary>
    public float Progress;

    /// <summary>Сколько единиц выхода уже сделано и ждёт забора.</summary>
    public int OutputBuffer;

    public bool IsCrafting => Progress > 0f;
}