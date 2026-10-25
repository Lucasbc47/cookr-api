namespace Cookr.Domain;

public class Ingredient
{
    public const int NameMaxLength = 100;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];
}
