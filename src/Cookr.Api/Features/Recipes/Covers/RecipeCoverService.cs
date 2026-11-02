using Cookr.Api.Shared;
using Cookr.Infrastructure;

namespace Cookr.Api.Features.Recipes.Covers;

public class RecipeCoverService(CookrDbContext dbContext, RecipeCoverStorage storage)
{
    private readonly CookrDbContext _dbContext = dbContext;
    private readonly RecipeCoverStorage _storage = storage;

    public async Task<Result<RecipeCoverResponse>> SetAsync(int recipeId, Stream content, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes.FindAsync([recipeId], ct);
        if (recipe is null) return Result<RecipeCoverResponse>.NotFound(RecipeErrors.NotFound);

        var webp = RecipeCoverImage.ToWebp(content);
        if (!webp.IsSuccess) return Result<RecipeCoverResponse>.Invalid(webp.Error);

        var fileName = await _storage.SaveAsync(recipeId, webp.Value, ct);
        var previous = recipe.ReplaceCover(fileName);
        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch
        {
            _storage.Delete(fileName);
            throw;
        }

        _storage.Delete(previous);
        return Result<RecipeCoverResponse>.Success(new RecipeCoverResponse($"{RecipeCoverStorage.RequestPath}/{fileName}"));
    }

    public async Task<Result> RemoveAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes.FindAsync([recipeId], ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        var previous = recipe.ReplaceCover(null);
        await _dbContext.SaveChangesAsync(ct);
        _storage.Delete(previous);
        return Result.Success();
    }
}
