using Cookr.Api.Features.Ingredients;
using Cookr.Api.Shared;
using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Recipes.RecipeIngredients;

public class RecipeIngredientService(CookrDbContext dbContext)
{
    private readonly CookrDbContext _dbContext = dbContext;

    public async Task<Result> AddAsync(int recipeId, AddRecipeIngredientRequest request, CancellationToken ct)
    {
        var recipe = await FindWithLinksAsync(recipeId, ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        if (!await _dbContext.Ingredients.AnyAsync(i => i.Id == request.IngredientId, ct))
        {
            return Result.NotFound(IngredientErrors.NotFound);
        }

        if (!recipe.AddIngredient(request.IngredientId, request.Quantity, request.Unit))
        {
            return Result.Conflict(RecipeIngredientErrors.AlreadyAdded);
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(int recipeId, int ingredientId, CancellationToken ct)
    {
        var recipe = await FindWithLinksAsync(recipeId, ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        if (!recipe.RemoveIngredient(ingredientId)) return Result.NotFound(RecipeIngredientErrors.NotInRecipe);

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Task<Recipe?> FindWithLinksAsync(int recipeId, CancellationToken ct) =>
        _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);
}
