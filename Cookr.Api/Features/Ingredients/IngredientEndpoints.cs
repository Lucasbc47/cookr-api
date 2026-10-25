using Cookr.Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Features.Ingredients;

public static class IngredientEndpoints
{
    public static IEndpointRouteBuilder MapIngredientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/ingredients").WithTags("Ingredients");

        group.MapGet("/", async (IIngredientService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)))
            .WithSummary("Lista ingredientes");

        group.MapGet("/{id:int}", async Task<Results<Ok<IngredientResponse>, ProblemHttpResult>> (
            int id, IIngredientService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .WithSummary("Ingrediente por id")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<IngredientResponse>, ProblemHttpResult>> (
            CreateIngredientRequest request, IIngredientService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return result.IsSuccess
                ? TypedResults.Created($"/ingredients/{result.Value.Id}", result.Value)
                : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<CreateIngredientRequest>>()
            .WithSummary("Cria ingrediente")
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async Task<Results<Ok<IngredientResponse>, ProblemHttpResult>> (
            int id, UpdateIngredientRequest request, IIngredientService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<UpdateIngredientRequest>>()
            .WithSummary("Atualiza ingrediente")
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, IIngredientService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAsync(id, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Remove ingrediente")
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
