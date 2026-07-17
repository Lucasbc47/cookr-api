namespace Cookr.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes");

        group.MapGet("/", async (IRecipeService service) =>
        {
            var recipes = await service.GetAllAsync();
            return Results.Ok(recipes.Select(r => r.ToSummary()));
        });

        group.MapGet("/{id:int}", async (int id, IRecipeService service) =>
        {
            var recipe = await service.GetByIdAsync(id);
            return recipe is not null
                ? Results.Ok(recipe.ToResponse())
                : Results.NotFound();
        });

        group.MapPost("/", async (CreateRecipeRequest request, IRecipeService service) =>
        {
            var created = await service.CreateAsync(request.ToEntity());
            return Results.Created($"/recipes/{created.Id}", created.ToResponse());
        });

        group.MapPut("/{id:int}", async (int id, UpdateRecipeRequest request, IRecipeService service) =>
        {
            var updated = await service.UpdateAsync(id, request);
            return updated is not null
                ? Results.Ok(updated.ToResponse())
                : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IRecipeService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
