namespace MyEngine.Crafting;

public sealed class RecipeRegistry
{
    private readonly Dictionary<string, Recipe> _recipes = new();

    public void Register(Recipe recipe)
    {
        _recipes[recipe.Id] = recipe;
    }

    public Recipe? Get(string id) => _recipes.GetValueOrDefault(id);
    public bool Has(string id) => _recipes.ContainsKey(id);
    public IEnumerable<Recipe> All => _recipes.Values;
    public void Clear() => _recipes.Clear();
}