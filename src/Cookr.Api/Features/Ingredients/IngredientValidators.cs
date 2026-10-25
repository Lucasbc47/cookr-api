using Cookr.Domain;
using FluentValidation;

namespace Cookr.Api.Features.Ingredients;

public class CreateIngredientRequestValidator : AbstractValidator<CreateIngredientRequest>
{
    public CreateIngredientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Ingredient.NameMaxLength);
    }
}

public class UpdateIngredientRequestValidator : AbstractValidator<UpdateIngredientRequest>
{
    public UpdateIngredientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Ingredient.NameMaxLength);
    }
}
