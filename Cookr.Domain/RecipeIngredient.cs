namespace Cookr.Domain;

public class RecipeIngredient
{
    public const int UnitMaxLength = 20;

    private RecipeIngredient() { }

    internal RecipeIngredient(int ingredientId, decimal quantity, string unit)
    {
        IngredientId = ingredientId;
        Quantity = quantity;
        Unit = unit;
    }

#pragma warning disable S1144
    public int RecipeId { get; private set; }
#pragma warning restore S1144
    public Recipe Recipe { get; private set; } = null!;
    public int IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
}
