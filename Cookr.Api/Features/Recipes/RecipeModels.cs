namespace Cookr.Api.Features.Recipes;

public record CreateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings);
public record UpdateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings);
public record RecipeSummary(int Id, string Title);
public record RecipeResponse(int Id, string Title, string Instructions, int PrepTimeMinutes, int Servings, List<RecipeIngredientResponse> Ingredients);


public record AddRecipeIngredientRequest(int IngredientId, decimal Quantity, string Unit);
public record RecipeIngredientResponse(int IngredientId, string Name, decimal Quantity, string Unit);

public enum AddIngredientResult
{
    Success,
    RecipeNotFound,
    IngredientNotFound,
    AlreadyAdded
}