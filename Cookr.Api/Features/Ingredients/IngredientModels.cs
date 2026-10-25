using Cookr.Api.Shared;

namespace Cookr.Api.Features.Ingredients;

public record CreateIngredientRequest(string Name);
public record UpdateIngredientRequest(string Name);
public record IngredientResponse(int Id, string Name);

public static class IngredientErrors
{
    public static readonly Error NotFound = new("ingredient_not_found", "Ingredient not found");
    public static readonly Error NameTaken = new("ingredient_name_taken", "An ingredient with this name already exists");
    public static readonly Error InUse = new("ingredient_in_use", "Ingredient is used by one or more recipes");
}
