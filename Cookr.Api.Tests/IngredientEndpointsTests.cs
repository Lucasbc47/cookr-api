using System.Net;
using System.Net.Http.Json;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Domain;

namespace Cookr.Api.Tests;

public sealed class IngredientEndpointsTests : IDisposable
{
    private readonly CookrApiFactory _factory = new();
    private readonly HttpClient _client;

    public IngredientEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Post_duplicate_name_ignoring_case_returns_conflict()
    {
        await _client.PostAsJsonAsync("/ingredients", new CreateIngredientRequest("Tomate"));

        var response = await _client.PostAsJsonAsync("/ingredients", new CreateIngredientRequest(" tomate "));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("ingredient_name_taken", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Put_with_same_name_keeps_working()
    {
        var created = await CreateIngredientAsync("Sal");

        var response = await _client.PutAsJsonAsync($"/ingredients/{created.Id}", new UpdateIngredientRequest("SAL"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ingredient_in_use_returns_conflict()
    {
        var ingredient = await CreateIngredientAsync("Farinha");
        var recipeResponse = await _client.PostAsJsonAsync("/recipes", new CreateRecipeRequest("Pao", "...", 90, 4));
        var recipe = (await recipeResponse.Content.ReadFromJsonAsync<RecipeResponse>())!;
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/ingredients", new AddRecipeIngredientRequest(ingredient.Id, 500, MeasurementUnit.Gram));

        var response = await _client.DeleteAsync($"/ingredients/{ingredient.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("ingredient_in_use", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Delete_unused_ingredient_returns_no_content()
    {
        var ingredient = await CreateIngredientAsync("Acucar");

        var response = await _client.DeleteAsync($"/ingredients/{ingredient.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<IngredientResponse> CreateIngredientAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/ingredients", new CreateIngredientRequest(name));
        return (await response.Content.ReadFromJsonAsync<IngredientResponse>())!;
    }
}
