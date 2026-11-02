using System.Globalization;
using Cookr.Api.Features.Recipes.Covers;
using Cookr.Api.Shared;
using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Recipes;

public class RecipeService(CookrDbContext dbContext, RecipeCoverStorage covers)
{
    private readonly CookrDbContext _dbContext = dbContext;
    private readonly RecipeCoverStorage _covers = covers;

    private static readonly StringComparer TitleOrder =
        StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: true);

    public async Task<List<RecipeSummary>> GetAllAsync(CancellationToken ct)
    {
        var recipes = await _dbContext.Recipes
            .Select(r => new { r.Id, r.Title, r.PrepTimeMinutes, r.Servings, r.CoverFileName })
            .ToListAsync(ct);
        return recipes
            .OrderBy(r => r.Title, TitleOrder)
            .Select(r => new RecipeSummary(r.Id, r.Title, r.PrepTimeMinutes, r.Servings, RecipeCoverStorage.UrlFor(r.CoverFileName)))
            .ToList();
    }

    public async Task<Result<RecipeResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var recipe = await FindWithIngredientsAsync(id, ct);
        return recipe is not null
            ? Result<RecipeResponse>.Success(recipe.ToResponse())
            : Result<RecipeResponse>.NotFound(RecipeErrors.NotFound);
    }

    public async Task<Result<RecipeResponse>> CreateAsync(CreateRecipeRequest request, CancellationToken ct)
    {
        if (await TitleTakenAsync(request.Title, null, ct)) return Result<RecipeResponse>.Conflict(RecipeErrors.TitleTaken);

        var recipe = request.ToEntity();
        recipe.ReplaceIngredients(await ToDraftsAsync(request.Ingredients, ct));
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync(ct);
        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }

    public async Task<Result<RecipeResponse>> UpdateAsync(int id, UpdateRecipeRequest request, CancellationToken ct)
    {
        var recipe = await FindWithIngredientsAsync(id, ct);
        if (recipe is null) return Result<RecipeResponse>.NotFound(RecipeErrors.NotFound);
        if (await TitleTakenAsync(request.Title, id, ct)) return Result<RecipeResponse>.Conflict(RecipeErrors.TitleTaken);

        request.ApplyTo(recipe);
        recipe.ReplaceIngredients(await ToDraftsAsync(request.Ingredients, ct));
        await _dbContext.SaveChangesAsync(ct);
        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var recipe = await _dbContext.Recipes.FindAsync([id], ct);
        if (recipe is null) return Result.NotFound(RecipeErrors.NotFound);

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync(ct);
        _covers.Delete(recipe.CoverFileName);
        return Result.Success();
    }

    private Task<bool> TitleTakenAsync(string title, int? exceptId, CancellationToken ct)
    {
        var trimmed = title.Trim();
        return _dbContext.Recipes.AnyAsync(r => r.Title == trimmed && r.Id != exceptId, ct);
    }

    private async Task<List<RecipeIngredientDraft>> ToDraftsAsync(List<RecipeIngredientInput> inputs, CancellationToken ct)
    {
        var names = inputs.Select(i => i.Name.Trim()).ToList();
        var known = await _dbContext.Ingredients.Where(i => names.Contains(i.Name)).ToListAsync(ct);

        return inputs
            .Select(input =>
            {
                var name = input.Name.Trim();
                var ingredient = known.Find(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase))
                    ?? new Ingredient(name);
                return new RecipeIngredientDraft(ingredient, input.Quantity, input.Unit);
            })
            .ToList();
    }

    private Task<Recipe?> FindWithIngredientsAsync(int id, CancellationToken ct) =>
        _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
}
