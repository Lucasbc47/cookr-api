using Cookr.Api.Shared;
using Cookr.Domain;

namespace Cookr.Api.Features.Recipes.RecipeIngredients;

public record AddRecipeIngredientRequest(int IngredientId, decimal Quantity, MeasurementUnit Unit);

public static class RecipeIngredientErrors
{
    public static readonly Error AlreadyAdded = new("recipe_ingredient_already_added", "Ingredient already added to this recipe");
    public static readonly Error NotInRecipe = new("recipe_ingredient_not_found", "Ingredient not linked to this recipe");
}
