namespace Cookr.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes").WithTags("Recipes");

        group.MapGet("/", async (IRecipeService service) =>
            Results.Ok(await service.GetAllAsync()))
            .WithSummary("Lista receitas (id + título)")
            .Produces<List<RecipeSummary>>();

        group.MapGet("/{id:int}", async (int id, IRecipeService service) =>
        {
            var recipe = await service.GetByIdAsync(id);
            return recipe is not null
                ? Results.Ok(recipe.ToResponse())
                : Results.NotFound();
        })
            .WithSummary("Receita completa com ingredientes")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRecipeRequest request, IRecipeService service) =>
        {
            var created = await service.CreateAsync(request.ToEntity());
            return Results.Created($"/recipes/{created.Id}", created.ToResponse());
        })
            .AddEndpointFilter<ValidationFilter<CreateRecipeRequest>>()
            .WithSummary("Cria receita")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (int id, UpdateRecipeRequest request, IRecipeService service) =>
        {
            var updated = await service.UpdateAsync(id, request);
            return updated is not null
                ? Results.Ok(updated.ToResponse())
                : Results.NotFound();
        })
            .AddEndpointFilter<ValidationFilter<UpdateRecipeRequest>>()
            .WithSummary("Atualiza receita")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async (int id, IRecipeService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithSummary("Remove receita")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/ingredients", async (int id, AddRecipeIngredientRequest request, IRecipeService service) =>
        {
            var result = await service.AddIngredientAsync(id, request);

            return result switch
            {
                AddIngredientResult.Success => Results.NoContent(),
                AddIngredientResult.RecipeNotFound => Results.NotFound("Recipe not found"),
                AddIngredientResult.IngredientNotFound => Results.NotFound("Ingredient not found"),
                AddIngredientResult.AlreadyAdded => Results.Conflict(),
                _ => Results.Problem()
            };
        })
            .AddEndpointFilter<ValidationFilter<AddRecipeIngredientRequest>>()
            .WithSummary("Vincula ingrediente à receita (quantity + unit)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}/ingredients/{ingredientId:int}", async (int id, int ingredientId, IRecipeService service) =>
        {
            var deleted = await service.RemoveIngredientAsync(id, ingredientId);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithSummary("Desvincula ingrediente da receita")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
