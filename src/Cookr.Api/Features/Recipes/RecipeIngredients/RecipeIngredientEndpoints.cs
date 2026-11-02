using Cookr.Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Features.Recipes.RecipeIngredients;

public static class RecipeIngredientEndpoints
{
    public static IEndpointRouteBuilder MapRecipeIngredientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes/{id:int}/ingredients").WithTags(RecipeEndpoints.Tag);

        group.MapPost("/", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, AddRecipeIngredientRequest request, RecipeIngredientService service, CancellationToken ct) =>
        {
            var result = await service.AddAsync(id, request, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .AddEndpointFilter<ValidationFilter<AddRecipeIngredientRequest>>()
            .WithSummary("Vincula ingrediente à receita")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{ingredientId:int}", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, int ingredientId, RecipeIngredientService service, CancellationToken ct) =>
        {
            var result = await service.RemoveAsync(id, ingredientId, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Desvincula ingrediente da receita")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
