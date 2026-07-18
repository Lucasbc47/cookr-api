using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Recipes;

public interface IRecipeService
{
    Task<List<RecipeSummary>> GetAllAsync();
    Task<Recipe?> GetByIdAsync(int id);
    Task<Recipe> CreateAsync(Recipe recipe);
    Task<Recipe?> UpdateAsync(int id, UpdateRecipeRequest request);
    Task<bool> DeleteAsync(int id);

    // RecipeIngredient
    Task<AddIngredientResult> AddIngredientAsync(int recipeId, AddRecipeIngredientRequest request);
    Task<bool> RemoveIngredientAsync(int recipeId, int ingredientId);
}

public class RecipeService(CookrDbContext dbContext) : IRecipeService
{
    private readonly CookrDbContext _dbContext = dbContext;
    public async Task<List<RecipeSummary>> GetAllAsync()
    {
        return await _dbContext.Recipes
            .Select(r => new RecipeSummary(r.Id, r.Title))
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
        await _dbContext.SaveChangesAsync();
        return recipe;
    }

    public async Task<Recipe?> UpdateAsync(int id, UpdateRecipeRequest request)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id);
        if (recipe is null) return null;

        request.ApplyTo(recipe);
        await _dbContext.SaveChangesAsync();
        return recipe;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id);
        if (recipe is null) return false;

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<AddIngredientResult> AddIngredientAsync(int recipeId, AddRecipeIngredientRequest request)
    {
        var recipe = await _dbContext.Recipes.FindAsync(recipeId);
        if (recipe is null) return AddIngredientResult.RecipeNotFound;

        var ingredient = await _dbContext.Ingredients.FindAsync(request.IngredientId);
        if (ingredient is null) return AddIngredientResult.IngredientNotFound;

        var alreadyAdded = await _dbContext.RecipeIngredients
            .AnyAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == request.IngredientId);

        if (alreadyAdded) return AddIngredientResult.AlreadyAdded;
        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = recipeId,
            IngredientId = request.IngredientId,
            Quantity = request.Quantity,
            Unit = request.Unit
        };
        _dbContext.RecipeIngredients.Add(recipeIngredient);
        await _dbContext.SaveChangesAsync();
        return AddIngredientResult.Success;

    }
    public async Task<bool> RemoveIngredientAsync(int recipeId, int ingredientId)
    {
        var link = await _dbContext.RecipeIngredients
            .FirstOrDefaultAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId);
        if (link is null) return false;

        _dbContext.RecipeIngredients.Remove(link);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
