using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Shared;

public static class DevelopmentDatabase
{
    // Aplica migrations e cria dados de exemplo, pra quem clona o repo já ter algo no Swagger
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CookrDbContext>();

        await dbContext.Database.MigrateAsync();
        if (await dbContext.Recipes.AnyAsync()) return;

        var carrot = new Ingredient("Cenoura");
        var egg = new Ingredient("Ovo");
        var flour = new Ingredient("Farinha de trigo");
        var sugar = new Ingredient("Açúcar");
        dbContext.Ingredients.AddRange(carrot, egg, flour, sugar);
        await dbContext.SaveChangesAsync();

        var cake = new Recipe(
            "Bolo de cenoura",
            "Bata cenoura, ovos e óleo no liquidificador. Misture com os secos. Asse 40 min a 180°C.",
            60,
            8);
        cake.AddIngredient(carrot.Id, 3, "un");
        cake.AddIngredient(egg.Id, 3, "un");
        cake.AddIngredient(flour.Id, 2, "xícara");
        cake.AddIngredient(sugar.Id, 1.5m, "xícara");

        dbContext.Recipes.Add(cake);
        await dbContext.SaveChangesAsync();
    }
}
