using Cookr.Domain;

namespace Cookr.Api.Features.Recipes;

public static class RecipeMappings
{
    public static Recipe ToEntity(this CreateRecipeRequest request) => new()
    {
        Title = request.Title,
        Instructions = request.Instructions,
        PrepTimeMinutes = request.PrepTimeMinutes,
        Servings = request.Servings
    };

    public static void ApplyTo(this UpdateRecipeRequest request, Recipe recipe)
    {
        recipe.Title = request.Title;
        recipe.Instructions = request.Instructions;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.Servings = request.Servings;
    }

    public static RecipeSummary ToSummary(this Recipe recipe) => new(recipe.Id, recipe.Title);

    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(recipe.Id, recipe.Title, recipe.Instructions, recipe.PrepTimeMinutes, recipe.Servings);
}
