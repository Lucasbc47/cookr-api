using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Ingredients;

public interface IIngredientService
{
    Task<List<Ingredient>> GetAllAsync();
    Task<Ingredient?> GetByIdAsync(int id);
    Task<Ingredient> CreateAsync(Ingredient ingredient);
    Task<Ingredient?> UpdateAsync(int id, UpdateIngredientRequest request);
    Task<bool> DeleteAsync(int id);
}

public class IngredientService(CookrDbContext dbContext) : IIngredientService
{
    private readonly CookrDbContext _dbContext = dbContext;

    public async Task<List<Ingredient>> GetAllAsync()
    {
        return await _dbContext.Ingredients.ToListAsync();
    }

    public async Task<Ingredient?> GetByIdAsync(int id)
    {
        return await _dbContext.Ingredients.FindAsync(id);
    }

    public async Task<Ingredient> CreateAsync(Ingredient ingredient)
    {
        await _dbContext.Ingredients.AddAsync(ingredient);
        await _dbContext.SaveChangesAsync();
        return ingredient;
    }

    public async Task<Ingredient?> UpdateAsync(int id, UpdateIngredientRequest request)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient is null) return null;

        request.ApplyTo(ingredient);
        await _dbContext.SaveChangesAsync();
        return ingredient;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient is null) return false;

        _dbContext.Ingredients.Remove(ingredient);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
