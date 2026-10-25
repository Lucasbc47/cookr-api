namespace Cookr.Domain;

public class Recipe
{
    public const int TitleMaxLength = 200;
    public const int InstructionsMaxLength = 4000;

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int Servings { get; set; }
    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];
}
