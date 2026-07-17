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

    public static RecipeSummary ToSummary(this Recipe recipe) => new(recipe.Id, recipe.Title);
}
