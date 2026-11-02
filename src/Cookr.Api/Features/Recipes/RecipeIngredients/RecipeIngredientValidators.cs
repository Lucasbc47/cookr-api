using FluentValidation;

namespace Cookr.Api.Features.Recipes.RecipeIngredients;

public class AddRecipeIngredientRequestValidator : AbstractValidator<AddRecipeIngredientRequest>
{
    public AddRecipeIngredientRequestValidator()
    {
        RuleFor(x => x.IngredientId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).IsInEnum();
    }
}
