using Cookr.Domain;
using FluentValidation;

namespace Cookr.Api.Features.Recipes;

public abstract class RecipeRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IRecipeRequest
{
    protected RecipeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Recipe.TitleMaxLength);
        RuleFor(x => x.Instructions).MaximumLength(Recipe.InstructionsMaxLength);
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.Servings).GreaterThan(0);
        RuleFor(x => x.Ingredients)
            .NotNull()
            .Must(HaveDistinctNames).WithMessage("Each ingredient can appear only once.");
        RuleForEach(x => x.Ingredients).SetValidator(new RecipeIngredientInputValidator());
    }

    private static bool HaveDistinctNames(List<RecipeIngredientInput>? ingredients) =>
        ingredients is null
        || ingredients.Select(i => i.Name?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ingredients.Count;
}

public class RecipeIngredientInputValidator : AbstractValidator<RecipeIngredientInput>
{
    public RecipeIngredientInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Ingredient.NameMaxLength);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).IsInEnum();
    }
}

public class CreateRecipeRequestValidator : RecipeRequestValidator<CreateRecipeRequest>;

public class UpdateRecipeRequestValidator : RecipeRequestValidator<UpdateRecipeRequest>;
