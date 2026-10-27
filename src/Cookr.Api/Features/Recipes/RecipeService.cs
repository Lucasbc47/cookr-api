using Cookr.Api.Features.Ingredients;
using Cookr.Api.Shared;
using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Recipes;

public class RecipeService(CookrDbContext dbContext)
{
    private readonly CookrDbContext _dbContext = dbContext;

    public async Task<List<RecipeSummary>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.Recipes
            .Select(r => new RecipeSummary(r.Id, r.Title, r.PrepTimeMinutes, r.Servings))
            .ToListAsync(ct);
    }

    public async Task<Result<RecipeResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var recipe = await FindWithIngredientsAsync(id, ct);
        return recipe is not null
            ? Result<RecipeResponse>.Success(recipe.ToResponse())
            : Result<RecipeResponse>.NotFound(RecipeErrors.NotFound);
    }

    public async Task<RecipeResponse> CreateAsync(CreateRecipeRequest request, CancellationToken ct)
    {
        var recipe = request.ToEntity();
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync(ct);
        return recipe.ToResponse();
    }

    public async Task<Result<RecipeResponse>> UpdateAsync(int id, UpdateRecipeRequest request, CancellationToken ct)
    {
        var recipe = await FindWithIngredientsAsync(id, ct);
        if (recipe is null) return Result<RecipeResponse>.NotFound(RecipeErrors.NotFound);

        request.ApplyTo(recipe);
        await _dbContext.SaveChangesAsync(ct);
        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes.FindAsync([id], ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> AddIngredientAsync(int recipeId, AddRecipeIngredientRequest request, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        if (!await _dbContext.Ingredients.AnyAsync(i => i.Id == request.IngredientId, ct))
        {
            return Result.NotFound(IngredientErrors.NotFound);
        }

        if (!recipe.AddIngredient(request.IngredientId, request.Quantity, request.Unit))
        {
            return Result.Conflict(RecipeErrors.IngredientAlreadyAdded);
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveIngredientAsync(int recipeId, int ingredientId, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        if (!recipe.RemoveIngredient(ingredientId)) return Result.NotFound(RecipeErrors.IngredientNotInRecipe);

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Task<Recipe?> FindWithIngredientsAsync(int id, CancellationToken ct) =>
        _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
}
