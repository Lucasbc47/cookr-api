namespace Cookr.Domain;

public class RecipeIngredient
{
    public const int UnitMaxLength = 20;

    private RecipeIngredient() { }

    internal RecipeIngredient(int ingredientId, decimal quantity, MeasurementUnit unit)
    {
        IngredientId = ingredientId;
        Quantity = quantity;
        Unit = unit;
    }

    internal RecipeIngredient(Ingredient ingredient, decimal quantity, MeasurementUnit unit)
    {
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Quantity = quantity;
        Unit = unit;
    }

    internal void Update(decimal quantity, MeasurementUnit unit)
    {
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
    public MeasurementUnit Unit { get; private set; }
}
