using Cookr.Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes").WithTags("Recipes");

        group.MapGet("/", async (IRecipeService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)))
            .WithSummary("Lista receitas (id + título)");

        group.MapGet("/{id:int}", async Task<Results<Ok<RecipeResponse>, ProblemHttpResult>> (
            int id, IRecipeService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .WithSummary("Receita completa com ingredientes")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRecipeRequest request, IRecipeService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/recipes/{created.Id}", created);
        })
            .AddEndpointFilter<ValidationFilter<CreateRecipeRequest>>()
            .WithSummary("Cria receita")
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async Task<Results<Ok<RecipeResponse>, ProblemHttpResult>> (
            int id, UpdateRecipeRequest request, IRecipeService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<UpdateRecipeRequest>>()
            .WithSummary("Atualiza receita")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, IRecipeService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAsync(id, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Remove receita")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/ingredients", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, AddRecipeIngredientRequest request, IRecipeService service, CancellationToken ct) =>
        {
            var result = await service.AddIngredientAsync(id, request, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<AddRecipeIngredientRequest>>()
            .WithSummary("Vincula ingrediente à receita (quantity + unit)")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}/ingredients/{ingredientId:int}", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, int ingredientId, IRecipeService service, CancellationToken ct) =>
        {
            var result = await service.RemoveIngredientAsync(id, ingredientId, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Desvincula ingrediente da receita")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
