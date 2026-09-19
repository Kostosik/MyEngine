namespace MyEngine.Crafting;

/// <summary>
/// Рецепт: что положить на вход, что получить на выходе, за сколько секунд.
/// </summary>
public sealed class Recipe
{
    /// <summary>Уникальный id рецепта: "iron_plate", "copper_wire".</summary>
    public string Id = "";

    /// <summary>Вход: itemId → количество.</summary>
    public Dictionary<string, int> Input = new();

    /// <summary>Выход: itemId → количество.</summary>
    public Dictionary<string, int> Output = new();

    /// <summary>Время производства в секундах.</summary>
    public float CraftTime = 1f;
}