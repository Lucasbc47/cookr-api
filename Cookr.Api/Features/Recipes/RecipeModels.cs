namespace Cookr.Api.Features.Recipes;

public record CreateRecipeRequest(string Title, string Instructions, int PrepTimeMinutes, int Servings);

public record RecipeSummary(int Id, string Title);
