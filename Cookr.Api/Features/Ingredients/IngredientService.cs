using Cookr.Api.Shared;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Features.Ingredients;

public interface IIngredientService
{
    Task<List<IngredientResponse>> GetAllAsync(CancellationToken ct);
    Task<Result<IngredientResponse>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<IngredientResponse>> CreateAsync(CreateIngredientRequest request, CancellationToken ct);
    Task<Result<IngredientResponse>> UpdateAsync(int id, UpdateIngredientRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}

public class IngredientService(CookrDbContext dbContext) : IIngredientService
{
    private const string NotFoundError = "Ingredient not found";

    private readonly CookrDbContext _dbContext = dbContext;

    public async Task<List<IngredientResponse>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.Ingredients
            .Select(i => new IngredientResponse(i.Id, i.Name))
            .ToListAsync(ct);
    }

    public async Task<Result<IngredientResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync([id], ct);
        return ingredient is not null
            ? Result<IngredientResponse>.Success(ingredient.ToResponse())
            : Result<IngredientResponse>.NotFound(NotFoundError);
    }

    public async Task<Result<IngredientResponse>> CreateAsync(CreateIngredientRequest request, CancellationToken ct)
    {
        var ingredient = request.ToEntity();
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync(ct);
        return Result<IngredientResponse>.Success(ingredient.ToResponse());
    }

    public async Task<Result<IngredientResponse>> UpdateAsync(int id, UpdateIngredientRequest request, CancellationToken ct)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync([id], ct);
        if (ingredient is null) return Result<IngredientResponse>.NotFound(NotFoundError);

        request.ApplyTo(ingredient);
        await _dbContext.SaveChangesAsync(ct);
        return Result<IngredientResponse>.Success(ingredient.ToResponse());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync([id], ct);
        if (ingredient is null) return Result.NotFound(NotFoundError);

        _dbContext.Ingredients.Remove(ingredient);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }
}
