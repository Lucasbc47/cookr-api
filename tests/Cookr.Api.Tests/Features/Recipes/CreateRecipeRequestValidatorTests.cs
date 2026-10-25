using Cookr.Api.Features.Recipes;
using FluentValidation.TestHelper;

namespace Cookr.Api.Tests.Features.Recipes;

public class CreateRecipeRequestValidatorTests
{
    private readonly CreateRecipeRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new CreateRecipeRequest("Bolo", "...", 40, 8);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyTitle_HasTitleError()
    {
        var request = new CreateRecipeRequest("", "...", 40, 8);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositivePrepTime_HasPrepTimeError(int prepTimeMinutes)
    {
        var request = new CreateRecipeRequest("Bolo", "...", prepTimeMinutes, 8);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.PrepTimeMinutes);
    }
}
