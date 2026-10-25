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

    private static Recipe CreateRecipe() => new("Bolo", "...", 40, 8);
}
