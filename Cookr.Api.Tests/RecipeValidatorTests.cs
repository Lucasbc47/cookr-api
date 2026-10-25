using Cookr.Api.Features.Recipes;

namespace Cookr.Api.Tests;

public class RecipeValidatorTests
{
    [Fact]
    public void Should_fail_when_title_empty()
    {
        var validator = new CreateRecipeRequestValidator();
        var request = new CreateRecipeRequest("", "1.2.3.", 10, 2);

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }
}