using Cookr.Domain;

namespace Cookr.Api.Tests;

public class RecipeTests
{
    [Fact]
    public void AddIngredient_twice_is_rejected()
    {
        var recipe = new Recipe("Bolo", "...", 40, 8);

        Assert.True(recipe.AddIngredient(1, 3, "un"));
        Assert.False(recipe.AddIngredient(1, 2, "un"));
        Assert.Single(recipe.RecipeIngredients);
    }

    [Fact]
    public void RemoveIngredient_not_in_recipe_is_rejected()
    {
        var recipe = new Recipe("Bolo", "...", 40, 8);

        Assert.False(recipe.RemoveIngredient(1));
    }
}
