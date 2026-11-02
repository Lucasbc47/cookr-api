namespace Cookr.Domain;

public class Recipe
{
    public const int TitleMaxLength = 200;
    public const int InstructionsMaxLength = 4000;
    public const int CoverFileNameMaxLength = 100;

    private readonly List<RecipeIngredient> _recipeIngredients = [];

    private Recipe() { }

    public Recipe(string title, string instructions, int prepTimeMinutes, int servings)
    {
        Update(title, instructions, prepTimeMinutes, servings);
    }

#pragma warning disable S1144
    public int Id { get; private set; }
#pragma warning restore S1144
    public string Title { get; private set; } = string.Empty;
    public string Instructions { get; private set; } = string.Empty;
    public int PrepTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public string? CoverFileName { get; private set; }
    public IReadOnlyCollection<RecipeIngredient> RecipeIngredients => _recipeIngredients.AsReadOnly();

    public void Update(string title, string instructions, int prepTimeMinutes, int servings)
    {
        Title = title.Trim();
        Instructions = instructions.Trim();
        PrepTimeMinutes = prepTimeMinutes;
        Servings = servings;
    }

    public string? ReplaceCover(string? fileName)
    {
        var previous = CoverFileName;
        CoverFileName = fileName;
        return previous;
    }

    public void ReplaceIngredients(IReadOnlyCollection<RecipeIngredientDraft> drafts)
    {
        if (drafts.DistinctBy(d => d.Ingredient).Count() != drafts.Count)
        {
            throw new ArgumentException("Each ingredient can appear only once", nameof(drafts));
        }

        _recipeIngredients.RemoveAll(link => !drafts.Any(d => d.Ingredient.Id != 0 && d.Ingredient.Id == link.IngredientId));
        foreach (var draft in drafts)
        {
            var existing = draft.Ingredient.Id == 0
                ? null
                : _recipeIngredients.Find(link => link.IngredientId == draft.Ingredient.Id);
            if (existing is not null) existing.Update(draft.Quantity, draft.Unit);
            else _recipeIngredients.Add(new RecipeIngredient(draft.Ingredient, draft.Quantity, draft.Unit));
        }
    }

    public bool AddIngredient(int ingredientId, decimal quantity, MeasurementUnit unit)
    {
        if (_recipeIngredients.Exists(ri => ri.IngredientId == ingredientId)) return false;

        _recipeIngredients.Add(new RecipeIngredient(ingredientId, quantity, unit));
        return true;
    }

    public bool RemoveIngredient(int ingredientId)
    {
        var link = _recipeIngredients.Find(ri => ri.IngredientId == ingredientId);
        return link is not null && _recipeIngredients.Remove(link);
    }
}
