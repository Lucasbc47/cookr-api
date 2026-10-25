using Cookr.Domain;

namespace Cookr.Api.Features.Ingredients;

public static class IngredientMappings
{
    public static Ingredient ToEntity(this CreateIngredientRequest request) => new(request.Name);

    public static void ApplyTo(this UpdateIngredientRequest request, Ingredient ingredient) =>
        ingredient.Rename(request.Name);

    public static IngredientResponse ToResponse(this Ingredient ingredient) => new(ingredient.Id, ingredient.Name);
}
