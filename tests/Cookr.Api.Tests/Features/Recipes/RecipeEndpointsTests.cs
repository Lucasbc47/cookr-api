using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Domain;

namespace Cookr.Api.Tests.Features.Recipes;

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
    public async Task GetAll_ExistingRecipe_ReturnsPrepTimeAndServings()
    {
        var request = new CreateRecipeRequest("Bolo", "...", 40, 8, []);
        await _client.PostAsJsonAsync("/recipes", request, CookrApiFactory.Json);

        var response = await _client.GetAsync("/recipes");

        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummary>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = Assert.Single(recipes!);
        Assert.Equal(request.PrepTimeMinutes, summary.PrepTimeMinutes);
        Assert.Equal(request.Servings, summary.Servings);
        Assert.Null(summary.CoverUrl);
    }

    [Fact]
    public async Task GetAll_SeveralRecipes_ReturnsSortedByTitle()
    {
        await _client.CreateRecipeAsync("Pudim");
        await _client.CreateRecipeAsync("Pão de queijo");
        await _client.CreateRecipeAsync("arroz doce");
        await _client.CreateRecipeAsync("Bolo");

        var recipes = await _client.GetFromJsonAsync<List<RecipeSummary>>("/recipes", CookrApiFactory.Json);

        Assert.Equal(["arroz doce", "Bolo", "Pão de queijo", "Pudim"], recipes!.Select(r => r.Title));
    }

    [Fact]
    public async Task Post_WithIngredients_CreatesRecipeAndMissingIngredientsTogether()
    {
        await _client.CreateIngredientAsync("Ovo");
        var request = new CreateRecipeRequest("Bolo de cenoura", "...", 60, 8,
        [
            new("ovo", 3, MeasurementUnit.Unit),
            new(" Cenoura ", 3, MeasurementUnit.Unit),
            new("Farinha de trigo", 2, MeasurementUnit.Cup)
        ]);

        var response = await _client.PostAsJsonAsync("/recipes", request, CookrApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<RecipeResponse>(CookrApiFactory.Json);
        Assert.Equal(["Ovo", "Cenoura", "Farinha de trigo"], created!.Ingredients.Select(i => i.Name));
        Assert.All(created.Ingredients, i => Assert.True(i.IngredientId > 0));
        var catalog = await _client.GetFromJsonAsync<List<IngredientResponse>>("/ingredients", CookrApiFactory.Json);
        Assert.Equal(3, catalog!.Count);
    }

    [Fact]
    public async Task Post_InvalidIngredient_CreatesNothing()
    {
        var request = new CreateRecipeRequest("Bolo", "...", 40, 8,
        [
            new("Cenoura", 3, MeasurementUnit.Unit),
            new("Ovo", 0, MeasurementUnit.Unit)
        ]);

        var response = await _client.PostAsJsonAsync("/recipes", request, CookrApiFactory.Json);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty("ingredients[1].quantity", out _));
        Assert.Empty((await _client.GetFromJsonAsync<List<RecipeSummary>>("/recipes"))!);
        Assert.Empty((await _client.GetFromJsonAsync<List<IngredientResponse>>("/ingredients"))!);
    }

    [Fact]
    public async Task Post_RepeatedIngredientName_ReturnsValidationProblem()
    {
        var request = new CreateRecipeRequest("Bolo", "...", 40, 8,
        [
            new("Ovo", 3, MeasurementUnit.Unit),
            new("OVO ", 1, MeasurementUnit.Unit)
        ]);

        var response = await _client.PostAsJsonAsync("/recipes", request, CookrApiFactory.Json);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty("ingredients", out _));
    }

    [Fact]
    public async Task Post_DuplicateTitleIgnoringCase_ReturnsConflict()
    {
        await _client.CreateRecipeAsync("Bolo de cenoura");

        var response = await _client.PostAsJsonAsync(
            "/recipes", new CreateRecipeRequest(" bolo de CENOURA ", "...", 40, 8, []), CookrApiFactory.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("recipe_title_taken", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_NumberAsString_ReturnsBadRequest()
    {
        var body = new StringContent(
            """{ "title": "Bolo", "instructions": "...", "prepTimeMinutes": "40", "servings": 8, "ingredients": [] }""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/recipes", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("bad_request", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Put_IngredientList_ReplacesLinksInPlace()
    {
        var recipe = await _client.CreateRecipeAsync();
        var carrot = await _client.CreateIngredientAsync("Cenoura");
        var egg = await _client.CreateIngredientAsync("Ovo");
        await _client.AddIngredientAsync(recipe.Id, carrot.Id, 3, MeasurementUnit.Unit);
        await _client.AddIngredientAsync(recipe.Id, egg.Id, 2, MeasurementUnit.Unit);
        var request = new UpdateRecipeRequest("Bolo editado", "...", 50, 6,
        [
            new("Ovo", 4, MeasurementUnit.Unit),
            new("Leite", 200, MeasurementUnit.Milliliter)
        ]);

        var response = await _client.PutAsJsonAsync($"/recipes/{recipe.Id}", request, CookrApiFactory.Json);

        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>(CookrApiFactory.Json);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bolo editado", updated!.Title);
        Assert.Equal(["Ovo", "Leite"], updated.Ingredients.Select(i => i.Name));
        Assert.Equal(egg.Id, updated.Ingredients[0].IngredientId);
        Assert.Equal(4, updated.Ingredients[0].Quantity);
    }

    [Fact]
    public async Task Put_KeepingOwnTitle_ReturnsOk()
    {
        var recipe = await _client.CreateRecipeAsync("Bolo");

        var response = await _client.PutAsJsonAsync(
            $"/recipes/{recipe.Id}", new UpdateRecipeRequest("BOLO", "novo preparo", 40, 8, []), CookrApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Put_TitleOfAnotherRecipe_ReturnsConflict()
    {
        await _client.CreateRecipeAsync("Pudim");
        var recipe = await _client.CreateRecipeAsync("Bolo");

        var response = await _client.PutAsJsonAsync(
            $"/recipes/{recipe.Id}", new UpdateRecipeRequest("pudim", "...", 40, 8, []), CookrApiFactory.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("recipe_title_taken", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Get_RecipeWithIngredient_ReturnsUnitAsCamelCaseText()
    {
        var recipe = await _client.CreateRecipeAsync();
        var ingredient = await _client.CreateIngredientAsync("Leite");
        await _client.AddIngredientAsync(recipe.Id, ingredient.Id, 200, MeasurementUnit.Milliliter);

        var json = await _client.GetStringAsync($"/recipes/{recipe.Id}");

        Assert.Contains("\"unit\":\"milliliter\"", json);
    }

    [Fact]
    public async Task Get_MissingRecipe_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync("/recipes/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("recipe_not_found", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_InvalidRecipe_ReturnsErrorsByCamelCaseField()
    {
        var response = await _client.PostAsJsonAsync("/recipes", new CreateRecipeRequest("", "...", 0, 0, []), CookrApiFactory.Json);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("prepTimeMinutes", out var prepTimeErrors));
        Assert.Equal("'prepTimeMinutes' must be greater than '0'.", prepTimeErrors[0].GetString());
    }

    [Fact]
    public async Task Post_MissingIngredients_ReturnsValidationProblem()
    {
        var body = new StringContent(
            """{ "title": "Bolo", "instructions": "...", "prepTimeMinutes": 40, "servings": 8 }""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/recipes", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_InvalidRecipe_ReturnsValidationProblem()
    {
        var recipe = await _client.CreateRecipeAsync();

        var response = await _client.PutAsJsonAsync($"/recipes/{recipe.Id}", new UpdateRecipeRequest("", "...", 10, 0, []), CookrApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await CookrApiFactory.ErrorCodeAsync(response));
    }
}
