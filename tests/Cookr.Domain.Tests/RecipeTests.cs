namespace Cookr.Domain.Tests;

public class RecipeTests
{
    [Fact]
    public void AddIngredient_NewIngredient_ReturnsTrue()
    {
        var recipe = CreateRecipe();

        var added = recipe.AddIngredient(1, 3, MeasurementUnit.Unit);

        Assert.True(added);
        Assert.Single(recipe.RecipeIngredients);
    }

    [Fact]
    public void AddIngredient_AlreadyInRecipe_ReturnsFalse()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(1, 3, MeasurementUnit.Unit);

        var added = recipe.AddIngredient(1, 2, MeasurementUnit.Unit);

        Assert.False(added);
        Assert.Single(recipe.RecipeIngredients);
    }

    [Fact]
    public void RemoveIngredient_InRecipe_ReturnsTrue()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(1, 3, MeasurementUnit.Unit);

        var removed = recipe.RemoveIngredient(1);

        Assert.True(removed);
        Assert.Empty(recipe.RecipeIngredients);
    }

    [Fact]
    public void RemoveIngredient_NotInRecipe_ReturnsFalse()
    {
        var recipe = CreateRecipe();

        var removed = recipe.RemoveIngredient(1);

        Assert.False(removed);
    }

    [Fact]
    public void ReplaceCover_ExistingCover_ReturnsPreviousFileName()
    {
        var recipe = CreateRecipe();
        recipe.ReplaceCover("old.webp");

        var previous = recipe.ReplaceCover("new.webp");

        Assert.Equal("old.webp", previous);
        Assert.Equal("new.webp", recipe.CoverFileName);
    }

    [Fact]
    public void ReplaceCover_Null_RemovesCover()
    {
        var recipe = CreateRecipe();
        recipe.ReplaceCover("old.webp");

        recipe.ReplaceCover(null);

        Assert.Null(recipe.CoverFileName);
    }

    [Fact]
    public void ReplaceIngredients_NewList_AddsEachOnce()
    {
        var recipe = CreateRecipe();
        var drafts = new[]
        {
            new RecipeIngredientDraft(new Ingredient("Ovo"), 3, MeasurementUnit.Unit),
            new RecipeIngredientDraft(new Ingredient("Leite"), 200, MeasurementUnit.Milliliter)
        };

        recipe.ReplaceIngredients(drafts);

        Assert.Equal(["Ovo", "Leite"], recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name));
    }

    [Fact]
    public void ReplaceIngredients_SameIngredientTwice_Throws()
    {
        var recipe = CreateRecipe();
        var egg = new Ingredient("Ovo");

        var act = () => recipe.ReplaceIngredients([
            new RecipeIngredientDraft(egg, 3, MeasurementUnit.Unit),
            new RecipeIngredientDraft(egg, 1, MeasurementUnit.Unit)
        ]);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ReplaceIngredients_EmptyList_RemovesAll()
    {
        var recipe = CreateRecipe();
        recipe.ReplaceIngredients([new RecipeIngredientDraft(new Ingredient("Ovo"), 3, MeasurementUnit.Unit)]);

        recipe.ReplaceIngredients([]);

        Assert.Empty(recipe.RecipeIngredients);
    }

    private static Recipe CreateRecipe() => new("Bolo", "...", 40, 8);
}
