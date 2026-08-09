namespace Cookr.Api.Features.Ingredients;

public static class IngredientEndpoints
{
    public static IEndpointRouteBuilder MapIngredientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/ingredients").WithTags("Ingredients");

        group.MapGet("/", async (IIngredientService service) =>
        {
            var ingredients = await service.GetAllAsync();
            return Results.Ok(ingredients.Select(i => i.ToSummary()));
        })
            .WithSummary("Lista ingredientes")
            .Produces<List<IngredientSummary>>();

        group.MapGet("/{id:int}", async (int id, IIngredientService service) =>
        {
            var ingredient = await service.GetByIdAsync(id);
            return ingredient is not null
                ? Results.Ok(ingredient.ToResponse())
                : Results.NotFound();
        })
            .WithSummary("Ingrediente por id")
            .Produces<IngredientResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateIngredientRequest request, IIngredientService service) =>
        {
            var created = await service.CreateAsync(request.ToEntity());
            return Results.Created($"/ingredients/{created.Id}", created.ToResponse());
        })
            .AddEndpointFilter<ValidationFilter<CreateIngredientRequest>>()
            .WithSummary("Cria ingrediente")
            .Produces<IngredientResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (int id, UpdateIngredientRequest request, IIngredientService service) =>
        {
            var updated = await service.UpdateAsync(id, request);
            return updated is not null
                ? Results.Ok(updated.ToResponse())
                : Results.NotFound();
        })
            .AddEndpointFilter<ValidationFilter<UpdateIngredientRequest>>()
            .WithSummary("Atualiza ingrediente")
            .Produces<IngredientResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async (int id, IIngredientService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithSummary("Remove ingrediente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
