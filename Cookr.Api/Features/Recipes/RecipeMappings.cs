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

    public static RecipeIngredient ToEntity(this AddRecipeIngredientRequest request, int recipeId) => new()
    {
        RecipeId = recipeId,
        IngredientId = request.IngredientId,
        Quantity = request.Quantity,
        Unit = request.Unit
    };
    public static RecipeIngredientResponse ToResponse(this RecipeIngredient ri) =>
        new(ri.IngredientId, ri.Ingredient.Name, ri.Quantity, ri.Unit);

    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(
            recipe.Id,
            recipe.Title,
            recipe.Instructions,
            recipe.PrepTimeMinutes,
            recipe.Servings,
            recipe.RecipeIngredients.Select(ri => ri.ToResponse()).ToList());

}
