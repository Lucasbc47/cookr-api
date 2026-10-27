using Cookr.Api.Shared;
using Cookr.Domain;

namespace Cookr.Api.Features.Recipes;

public record CreateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings);
public record UpdateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings);
public record RecipeSummary(int Id, string Title, int PrepTimeMinutes, int Servings);
public record RecipeResponse(int Id, string Title, string Instructions, int PrepTimeMinutes, int Servings, List<RecipeIngredientResponse> Ingredients);

public record AddRecipeIngredientRequest(int IngredientId, decimal Quantity, MeasurementUnit Unit);
public record RecipeIngredientResponse(int IngredientId, string Name, decimal Quantity, MeasurementUnit Unit);

public static class RecipeErrors
{
    public static readonly Error NotFound = new("recipe_not_found", "Recipe not found");
    public static readonly Error IngredientAlreadyAdded = new("recipe_ingredient_already_added", "Ingredient already added to this recipe");
    public static readonly Error IngredientNotInRecipe = new("recipe_ingredient_not_found", "Ingredient not linked to this recipe");
}
