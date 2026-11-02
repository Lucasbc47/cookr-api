using Microsoft.Extensions.Options;

namespace Cookr.Api.Features.Recipes.Covers;

public class RecipeCoverStorage
{
    public const string RequestPath = "/covers";

    public RecipeCoverStorage(IOptions<RecipeCoverOptions> options, IWebHostEnvironment environment)
    {
        DirectoryPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.Path));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public static string? UrlFor(string? fileName) =>
        fileName is null ? null : $"{RequestPath}/{fileName}";

    public async Task<string> SaveAsync(int recipeId, byte[] webp, CancellationToken ct)
    {
        var fileName = $"{recipeId}-{Guid.NewGuid():N}.webp";
        await File.WriteAllBytesAsync(Path.Combine(DirectoryPath, fileName), webp, ct);
        return fileName;
    }

    public void Delete(string? fileName)
    {
        if (fileName is null) return;

        var path = Path.Combine(DirectoryPath, Path.GetFileName(fileName));
        if (File.Exists(path)) File.Delete(path);
    }
}
