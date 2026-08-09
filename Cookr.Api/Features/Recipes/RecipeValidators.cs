using FluentValidation;

namespace Cookr.Api.Features.Recipes;

public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.Servings).GreaterThan(0);
    }
}

public class UpdateRecipeRequestValidator : AbstractValidator<UpdateRecipeRequest>
{
    public UpdateRecipeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.Servings).GreaterThan(0);
    }
}

public class AddRecipeIngredientRequestValidator : AbstractValidator<AddRecipeIngredientRequest>
{
    public AddRecipeIngredientRequestValidator()
    {
        RuleFor(x => x.IngredientId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty();
    }
}