using Cookr.Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Cookr.Api.Features.Recipes.Covers;

public static class RecipeCoverEndpoints
{
    public static IServiceCollection AddRecipeCovers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RecipeCoverOptions>(configuration.GetSection(RecipeCoverOptions.Section));
        services.AddSingleton<RecipeCoverStorage>();
        return services.AddScoped<RecipeCoverService>();
    }

    public static WebApplication UseRecipeCoverFiles(this WebApplication app)
    {
        var storage = app.Services.GetRequiredService<RecipeCoverStorage>();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.DirectoryPath),
            RequestPath = RecipeCoverStorage.RequestPath,
            OnPrepareResponse = context =>
                context.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable"
        });
        return app;
    }

    public static IEndpointRouteBuilder MapRecipeCoverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recipes/{id:int}/cover").WithTags(RecipeEndpoints.Tag);

        group.MapPut("/", async Task<Results<Ok<RecipeCoverResponse>, ProblemHttpResult>> (
            int id, IFormFile file, RecipeCoverService service, CancellationToken ct) =>
        {
            await using var content = file.OpenReadStream();
            var result = await service.SetAsync(id, content, ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
        })
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(RecipeCoverImage.MaxUploadBytes + 1024 * 1024))
            .WithSummary("Envia ou troca a capa da receita")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

        group.MapDelete("/", async Task<Results<NoContent, ProblemHttpResult>> (
            int id, RecipeCoverService service, CancellationToken ct) =>
        {
            var result = await service.RemoveAsync(id, ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
            .WithSummary("Remove a capa da receita")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
