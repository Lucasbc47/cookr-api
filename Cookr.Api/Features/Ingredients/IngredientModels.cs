namespace Cookr.Api.Features.Ingredients;

public record CreateIngredientRequest(string Name);
public record UpdateIngredientRequest(string Name);
public record IngredientSummary(int Id, string Name);
public record IngredientResponse(int Id, string Name);
