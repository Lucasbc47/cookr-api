using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Domain;

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
        await _client.PostAsJsonAsync(
            $"/recipes/{recipe.Id}/ingredients",
            new AddRecipeIngredientRequest(ingredient.Id, 3, MeasurementUnit.Cup),
            CookrApiFactory.Json);

        var response = await _client.PutAsJsonAsync($"/recipes/{recipe.Id}", new UpdateRecipeRequest("Bolo editado", "...", 50, 6));

        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>(CookrApiFactory.Json);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bolo editado", updated!.Title);
        Assert.Single(updated.Ingredients);
        Assert.Equal("Cenoura", updated.Ingredients[0].Name);
        Assert.Equal(MeasurementUnit.Cup, updated.Ingredients[0].Unit);
    }

    [Fact]
    public async Task Unit_is_sent_as_camel_case_text()
    {
        var recipe = await CreateRecipeAsync();
        var ingredient = await CreateIngredientAsync("Leite");
        await _client.PostAsJsonAsync(
            $"/recipes/{recipe.Id}/ingredients",
            new AddRecipeIngredientRequest(ingredient.Id, 200, MeasurementUnit.Milliliter),
            CookrApiFactory.Json);

        var json = await _client.GetStringAsync($"/recipes/{recipe.Id}");

        Assert.Contains("\"unit\":\"milliliter\"", json);
    }

    [Fact]
    public async Task Unknown_unit_returns_bad_request()
    {
        var recipe = await CreateRecipeAsync();
        var ingredient = await CreateIngredientAsync("Sal");
        var body = new StringContent(
            $$"""{ "ingredientId": {{ingredient.Id}}, "quantity": 1, "unit": "banana" }""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync($"/recipes/{recipe.Id}/ingredients", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("bad_request", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Get_missing_recipe_returns_problem_details()
    {
        var response = await _client.GetAsync("/recipes/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("recipe_not_found", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_invalid_recipe_returns_errors_by_camel_case_field()
    {
        var response = await _client.PostAsJsonAsync("/recipes", new CreateRecipeRequest("", "...", 0, 0));

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("prepTimeMinutes", out var prepTimeErrors));
        Assert.Equal("'prepTimeMinutes' must be greater than '0'.", prepTimeErrors[0].GetString());
    }

    [Fact]
    public async Task Add_same_ingredient_twice_returns_conflict()
    {
        var recipe = await CreateRecipeAsync();
        var ingredient = await CreateIngredientAsync("Ovo");
        var request = new AddRecipeIngredientRequest(ingredient.Id, 2, MeasurementUnit.Unit);
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
