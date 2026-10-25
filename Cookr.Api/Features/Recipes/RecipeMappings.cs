using Cookr.Domain;

namespace Cookr.Api.Features.Recipes;

public static class RecipeMappings
{
    public static Recipe ToEntity(this CreateRecipeRequest request) =>
        new(request.Title, request.Instructions, request.PrepTimeMinutes, request.Servings);

    public static void ApplyTo(this UpdateRecipeRequest request, Recipe recipe) =>
        recipe.Update(request.Title, request.Instructions, request.PrepTimeMinutes, request.Servings);

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
