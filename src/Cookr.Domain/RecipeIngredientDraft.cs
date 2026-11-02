namespace Cookr.Domain;

public sealed record RecipeIngredientDraft(Ingredient Ingredient, decimal Quantity, MeasurementUnit Unit);
