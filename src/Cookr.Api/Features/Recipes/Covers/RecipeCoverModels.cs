using Cookr.Api.Shared;

namespace Cookr.Api.Features.Recipes.Covers;

public record RecipeCoverResponse(string CoverUrl);

public static class RecipeCoverErrors
{
    public static readonly Error InvalidImage = new("cover_invalid_image", "Cover must be a JPEG, PNG or WebP image");
    public static readonly Error TooLarge = new("cover_too_large", "Cover exceeds 10 MB or 40 megapixels");
}

public class RecipeCoverOptions
{
    public const string Section = "RecipeCovers";

    public string Path { get; set; } = "Data/covers";
}
