using System.Globalization;
using System.Text;
using Cookr.Api.Features.Recipes;
using Cookr.Api.Features.Recipes.Covers;
using Cookr.Domain;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Api.Shared;

public static class DevelopmentDatabase
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CookrDbContext>();
        await dbContext.Database.MigrateAsync();

        var recipes = scope.ServiceProvider.GetRequiredService<RecipeService>();
        var covers = scope.ServiceProvider.GetRequiredService<RecipeCoverService>();
        var coversFolder = Path.Combine(app.Environment.ContentRootPath, "SeedData", "covers");

        foreach (var recipe in SampleRecipes)
        {
            await recipes.CreateAsync(recipe, CancellationToken.None);

            var title = recipe.Title.Trim();
            var saved = await dbContext.Recipes.AsNoTracking().FirstAsync(r => r.Title == title);
            var coverPath = Path.Combine(coversFolder, $"{Slug(title)}.jpg");
            if (saved.CoverFileName is not null || !File.Exists(coverPath)) continue;

            await using var cover = File.OpenRead(coverPath);
            await covers.SetAsync(saved.Id, cover, CancellationToken.None);
        }
    }

    private static string Slug(string title) =>
        string.Concat(title.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant()
            .Replace(' ', '-');

    private static RecipeIngredientInput Item(string name, decimal quantity, MeasurementUnit unit) =>
        new(name, quantity, unit);

    private static readonly CreateRecipeRequest[] SampleRecipes =
    [
        new(
            "Bolo de cenoura",
            """
            Bata no liquidificador as cenouras, os ovos e o óleo até virar um creme liso.
            Numa tigela, misture o creme com o açúcar e a farinha.
            Junte o fermento por último, mexendo devagar.
            Asse em forma untada por cerca de 40 minutos a 180 °C.
            """,
            60,
            8,
            [
                Item("Cenoura", 3, MeasurementUnit.Unit),
                Item("Ovo", 3, MeasurementUnit.Unit),
                Item("Óleo", 1, MeasurementUnit.Cup),
                Item("Açúcar", 2, MeasurementUnit.Cup),
                Item("Farinha de trigo", 2, MeasurementUnit.Cup),
                Item("Fermento em pó", 1, MeasurementUnit.Tablespoon)
            ]),
        new(
            "Pão de queijo mineiro",
            """
            Ferva o leite com o óleo e o sal e escalde o polvilho, mexendo bem.
            Espere amornar e junte os ovos, um de cada vez.
            Acrescente o queijo e sove até a massa desgrudar das mãos.
            Faça bolinhas e asse a 200 °C por 25 minutos, até dourar.
            """,
            45,
            20,
            [
                Item("Polvilho azedo", 500, MeasurementUnit.Gram),
                Item("Leite", 250, MeasurementUnit.Milliliter),
                Item("Óleo", 0.5m, MeasurementUnit.Cup),
                Item("Ovo", 2, MeasurementUnit.Unit),
                Item("Queijo minas curado", 200, MeasurementUnit.Gram),
                Item("Sal", 1, MeasurementUnit.Teaspoon)
            ]),
        new(
            "Brigadeiro de panela",
            """
            Coloque o leite condensado, o chocolate e a manteiga na panela.
            Cozinhe em fogo baixo, mexendo sem parar, até desgrudar do fundo.
            Sirva de colher, ainda morno.
            """,
            20,
            6,
            [
                Item("Leite condensado", 1, MeasurementUnit.Unit),
                Item("Chocolate em pó", 3, MeasurementUnit.Tablespoon),
                Item("Manteiga", 1, MeasurementUnit.Tablespoon)
            ]),
        new(
            "Feijoada de domingo",
            """
            Deixe o feijão e as carnes salgadas de molho de um dia para o outro, trocando a água.
            Cozinhe o feijão com as carnes até ficarem macias.
            Refogue o alho e a cebola e junte ao feijão com a linguiça.
            Cozinhe mais 30 minutos em fogo baixo e acerte o sal.
            """,
            240,
            10,
            [
                Item("Feijão preto", 1, MeasurementUnit.Kilogram),
                Item("Carne seca", 500, MeasurementUnit.Gram),
                Item("Costelinha de porco", 500, MeasurementUnit.Gram),
                Item("Linguiça calabresa", 2, MeasurementUnit.Unit),
                Item("Cebola", 2, MeasurementUnit.Unit),
                Item("Alho", 4, MeasurementUnit.Unit),
                Item("Folha de louro", 3, MeasurementUnit.Unit)
            ]),
        new(
            "Moqueca de camarão",
            """
            Tempere o camarão com limão, alho e sal e deixe descansar por 15 minutos.
            Numa panela larga, faça camadas de cebola, tomate e pimentão.
            Coloque o camarão por cima e regue com o leite de coco e o azeite de dendê.
            Tampe e cozinhe em fogo médio por 15 minutos, sem mexer.
            Finalize com coentro picado.
            """,
            45,
            4,
            [
                Item("Camarão limpo", 500, MeasurementUnit.Gram),
                Item("Tomate", 3, MeasurementUnit.Unit),
                Item("Cebola", 1, MeasurementUnit.Unit),
                Item("Pimentão", 1, MeasurementUnit.Unit),
                Item("Leite de coco", 200, MeasurementUnit.Milliliter),
                Item("Azeite de dendê", 2, MeasurementUnit.Tablespoon),
                Item("Limão", 1, MeasurementUnit.Unit),
                Item("Coentro", 1, MeasurementUnit.Pinch)
            ]),
        new(
            "Pudim de leite condensado",
            """
            Derreta o açúcar numa forma de furo central até virar caramelo e espalhe pelas laterais.
            Bata no liquidificador o leite condensado, o leite e os ovos.
            Despeje na forma, cubra com papel alumínio e asse em banho-maria a 180 °C por 1 hora.
            Deixe esfriar e leve à geladeira por 4 horas antes de desenformar.
            """,
            90,
            10,
            [
                Item("Leite condensado", 1, MeasurementUnit.Unit),
                Item("Leite", 1, MeasurementUnit.Cup),
                Item("Ovo", 3, MeasurementUnit.Unit),
                Item("Açúcar", 1, MeasurementUnit.Cup)
            ]),
        new(
            "Vinagrete",
            """
            Pique o tomate, a cebola e o pimentão em cubos pequenos.
            Misture com o cheiro-verde, o vinagre e o azeite.
            Tempere com sal e deixe descansar 10 minutos antes de servir.
            """,
            15,
            6,
            [
                Item("Tomate", 3, MeasurementUnit.Unit),
                Item("Cebola", 1, MeasurementUnit.Unit),
                Item("Pimentão", 0.5m, MeasurementUnit.Unit),
                Item("Cheiro-verde", 2, MeasurementUnit.Tablespoon),
                Item("Vinagre", 3, MeasurementUnit.Tablespoon),
                Item("Azeite", 3, MeasurementUnit.Tablespoon),
                Item("Sal", 1, MeasurementUnit.Pinch)
            ]),
        new(
            "Arroz carreteiro",
            """
            Doure a carne seca dessalgada em cubos com a cebola e o alho.
            Junte o arroz e refogue por 2 minutos.
            Cubra com água quente e cozinhe em fogo baixo até secar.
            Finalize com cheiro-verde.
            """,
            40,
            6,
            [
                Item("Arroz", 2, MeasurementUnit.Cup),
                Item("Carne seca", 400, MeasurementUnit.Gram),
                Item("Cebola", 1, MeasurementUnit.Unit),
                Item("Alho", 3, MeasurementUnit.Unit),
                Item("Cheiro-verde", 2, MeasurementUnit.Tablespoon)
            ])
    ];
}
