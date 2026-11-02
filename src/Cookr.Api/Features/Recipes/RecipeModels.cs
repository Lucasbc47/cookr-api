using Cookr.Api.Shared;
using Cookr.Domain;

namespace Cookr.Api.Features.Recipes;

public interface IRecipeRequest
{
    string Title { get; }
    string Instructions { get; }
    int PrepTimeMinutes { get; }
    int Servings { get; }
    List<RecipeIngredientInput> Ingredients { get; }
}

public record RecipeIngredientInput(string Name, decimal Quantity, MeasurementUnit Unit);
public record CreateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings, List<RecipeIngredientInput> Ingredients) : IRecipeRequest;
public record UpdateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings, List<RecipeIngredientInput> Ingredients) : IRecipeRequest;
public record RecipeSummary(int Id, string Title, int PrepTimeMinutes, int Servings, string? CoverUrl);
public record RecipeResponse(int Id, string Title, string Instructions, int PrepTimeMinutes, int Servings, string? CoverUrl, List<RecipeIngredientResponse> Ingredients);
public record RecipeIngredientResponse(int IngredientId, string Name, decimal Quantity, MeasurementUnit Unit);

public static class RecipeErrors
{
    public static readonly Error NotFound = new("recipe_not_found", "Recipe not found");
    public static readonly Error TitleTaken = new("recipe_title_taken", "Another recipe already has this title");
}
