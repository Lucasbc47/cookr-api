using System.Net;
using Cookr.Domain;

namespace Cookr.Api.Tests.Features.Recipes.RecipeIngredients;

public sealed class RecipeIngredientEndpointsTests : IDisposable
{
    private readonly CookrApiFactory _factory = new();
    private readonly HttpClient _client;

    public RecipeIngredientEndpointsTests()
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
    public async Task Post_UnknownUnit_ReturnsBadRequest()
    {
        var recipe = await _client.CreateRecipeAsync();
        var ingredient = await _client.CreateIngredientAsync("Sal");
        var body = new StringContent(
            $$"""{ "ingredientId": {{ingredient.Id}}, "quantity": 1, "unit": "banana" }""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync($"/recipes/{recipe.Id}/ingredients", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("bad_request", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_SameIngredientTwice_ReturnsConflict()
    {
        var recipe = await _client.CreateRecipeAsync();
        var ingredient = await _client.CreateIngredientAsync("Ovo");
        await _client.AddIngredientAsync(recipe.Id, ingredient.Id, 2, MeasurementUnit.Unit);

        var response = await _client.AddIngredientAsync(recipe.Id, ingredient.Id, 2, MeasurementUnit.Unit);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("recipe_ingredient_already_added", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_UnknownIngredient_ReturnsNotFound()
    {
        var recipe = await _client.CreateRecipeAsync();

        var response = await _client.AddIngredientAsync(recipe.Id, 999, 1, MeasurementUnit.Unit);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("ingredient_not_found", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Delete_IngredientNotInRecipe_ReturnsNotFound()
    {
        var recipe = await _client.CreateRecipeAsync();

        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/ingredients/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("recipe_ingredient_not_found", await CookrApiFactory.ErrorCodeAsync(response));
    }
}
