using Cookr.Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public const string Tag = "Recipes";

    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes").WithTags(Tag);

        group.MapGet("/", async (RecipeService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)))
            .WithSummary("Lista receitas");

        group.MapGet("/{id:int}", async Task<Results<Ok<RecipeResponse>, ProblemHttpResult>> (
            int id, RecipeService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .WithSummary("Receita completa com ingredientes")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<RecipeResponse>, ProblemHttpResult>> (
            CreateRecipeRequest request, RecipeService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return result.IsSuccess
                ? TypedResults.Created($"/recipes/{result.Value.Id}", result.Value)
                : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<CreateRecipeRequest>>()
            .WithSummary("Cria receita com os ingredientes (por nome; cria no catálogo os que não existem)")
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async Task<Results<Ok<RecipeResponse>, ProblemHttpResult>> (
            int id, UpdateRecipeRequest request, RecipeService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<UpdateRecipeRequest>>()
            .WithSummary("Atualiza receita e substitui a lista de ingredientes")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, RecipeService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAsync(id, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Remove receita")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
