using System.Net.Http.Json;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Api.Features.Recipes.RecipeIngredients;
using Cookr.Domain;

namespace Cookr.Api.Tests;

internal static class HttpClientExtensions
{
    public static async Task<RecipeResponse> CreateRecipeAsync(this HttpClient client, string title = "Bolo")
    {
        var response = await client.PostAsJsonAsync("/recipes", new CreateRecipeRequest(title, "...", 40, 8, []), CookrApiFactory.Json);
        return (await response.Content.ReadFromJsonAsync<RecipeResponse>(CookrApiFactory.Json))!;
    }

    public static async Task<IngredientResponse> CreateIngredientAsync(this HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/ingredients", new CreateIngredientRequest(name));
        return (await response.Content.ReadFromJsonAsync<IngredientResponse>(CookrApiFactory.Json))!;
    }

    public static Task<HttpResponseMessage> AddIngredientAsync(
        this HttpClient client, int recipeId, int ingredientId, decimal quantity, MeasurementUnit unit) =>
        client.PostAsJsonAsync(
            $"/recipes/{recipeId}/ingredients",
            new AddRecipeIngredientRequest(ingredientId, quantity, unit),
            CookrApiFactory.Json);
}
