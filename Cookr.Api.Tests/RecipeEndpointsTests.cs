using System.Net;
using System.Net.Http.Json;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;

namespace Cookr.Api.Tests;

public sealed class RecipeEndpointsTests : IDisposable
{
    private readonly CookrApiFactory _factory = new();
    private readonly HttpClient _client;

    public RecipeEndpointsTests()
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
    public async Task Put_returns_recipe_with_ingredients()
    {
        var recipe = await CreateRecipeAsync();
        var ingredient = await CreateIngredientAsync("Cenoura");
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/ingredients", new AddRecipeIngredientRequest(ingredient.Id, 3, "un"));

        var response = await _client.PutAsJsonAsync($"/recipes/{recipe.Id}", new UpdateRecipeRequest("Bolo editado", "...", 50, 6));

        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bolo editado", updated!.Title);
        Assert.Single(updated.Ingredients);
        Assert.Equal("Cenoura", updated.Ingredients[0].Name);
    }

    [Fact]
    public async Task Get_missing_recipe_returns_problem_details()
    {
        var response = await _client.GetAsync("/recipes/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Post_invalid_recipe_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/recipes", new CreateRecipeRequest("", "...", 0, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Add_same_ingredient_twice_returns_conflict()
    {
        var recipe = await CreateRecipeAsync();
        var ingredient = await CreateIngredientAsync("Ovo");
        var request = new AddRecipeIngredientRequest(ingredient.Id, 2, "un");
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/ingredients", request);

        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/ingredients", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<RecipeResponse> CreateRecipeAsync()
    {
        var response = await _client.PostAsJsonAsync("/recipes", new CreateRecipeRequest("Bolo", "...", 40, 8));
        return (await response.Content.ReadFromJsonAsync<RecipeResponse>())!;
    }

    private async Task<IngredientResponse> CreateIngredientAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/ingredients", new CreateIngredientRequest(name));
        return (await response.Content.ReadFromJsonAsync<IngredientResponse>())!;
    }
}
