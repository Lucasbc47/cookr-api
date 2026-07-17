using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Recipes;

public interface IRecipeService
{
    Task<List<Recipe>> GetAllAsync();
    Task<Recipe?> GetByIdAsync(int id);
    Task<Recipe> CreateAsync(Recipe recipe);
    Task<bool> DeleteAsync(int id);
}

public class RecipeService(CookrDbContext dbContext) : IRecipeService
{
    private readonly CookrDbContext _dbContext = dbContext;

    public async Task<List<Recipe>> GetAllAsync()
    {
        return await _dbContext.Recipes.ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _dbContext.Recipes.FindAsync(id);
    }
    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
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

}
