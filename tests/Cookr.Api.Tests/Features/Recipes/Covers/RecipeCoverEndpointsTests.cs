using System.Net;
using System.Net.Http.Json;
using Cookr.Api.Features.Recipes;
using Cookr.Api.Features.Recipes.Covers;

namespace Cookr.Api.Tests.Features.Recipes.Covers;

public sealed class RecipeCoverEndpointsTests : IDisposable
{
    private readonly CookrApiFactory _factory = new();
    private readonly HttpClient _client;

    public RecipeCoverEndpointsTests()
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
    public async Task Put_ValidImage_ServesCachedWebp()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload(TestImages.Png(1200, 900));

        var response = await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cover = await response.Content.ReadFromJsonAsync<RecipeCoverResponse>();
        var file = await _client.GetAsync(cover!.CoverUrl);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Equal("image/webp", file.Content.Headers.ContentType?.MediaType);
        Assert.Contains("immutable", file.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Put_ValidImage_ExposesCoverUrlOnRecipe()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload(TestImages.Png(400, 300));
        var put = await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);
        var cover = await put.Content.ReadFromJsonAsync<RecipeCoverResponse>();

        var detail = await _client.GetFromJsonAsync<RecipeResponse>($"/recipes/{recipe.Id}", CookrApiFactory.Json);

        var summaries = await _client.GetFromJsonAsync<List<RecipeSummary>>("/recipes", CookrApiFactory.Json);
        Assert.Equal(cover!.CoverUrl, detail!.CoverUrl);
        Assert.Equal(cover.CoverUrl, Assert.Single(summaries!).CoverUrl);
    }

    [Fact]
    public async Task Put_NotAnImage_ReturnsInvalidImage()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload("hello"u8.ToArray(), "notes.png");

        var response = await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("cover_invalid_image", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Put_FileOverTenMegabytes_ReturnsTooLarge()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload(new byte[RecipeCoverImage.MaxUploadBytes + 1]);

        var response = await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("cover_too_large", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Put_MissingRecipe_ReturnsNotFound()
    {
        using var upload = TestImages.Upload(TestImages.Png(10, 10));

        var response = await _client.PutAsync("/recipes/999/cover", upload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("recipe_not_found", await CookrApiFactory.ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Put_ReplacingCover_DeletesPreviousFile()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var first = TestImages.Upload(TestImages.Png(10, 10));
        await _client.PutAsync($"/recipes/{recipe.Id}/cover", first);
        using var second = TestImages.Upload(TestImages.Png(20, 20));

        await _client.PutAsync($"/recipes/{recipe.Id}/cover", second);

        Assert.Single(Directory.GetFiles(_factory.CoversPath));
    }

    [Fact]
    public async Task Delete_RecipeWithCover_ClearsCoverAndFile()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload(TestImages.Png(10, 10));
        await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);

        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/cover");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var detail = await _client.GetFromJsonAsync<RecipeResponse>($"/recipes/{recipe.Id}", CookrApiFactory.Json);
        Assert.Null(detail!.CoverUrl);
        Assert.Empty(Directory.GetFiles(_factory.CoversPath));
    }

    [Fact]
    public async Task DeleteRecipe_WithCover_DeletesFile()
    {
        var recipe = await _client.CreateRecipeAsync();
        using var upload = TestImages.Upload(TestImages.Png(10, 10));
        await _client.PutAsync($"/recipes/{recipe.Id}/cover", upload);

        await _client.DeleteAsync($"/recipes/{recipe.Id}");

        Assert.Empty(Directory.GetFiles(_factory.CoversPath));
    }
}
