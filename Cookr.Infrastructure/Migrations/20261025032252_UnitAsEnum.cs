using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnitAsEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "RecipeIngredients" SET "Unit" = CASE lower(trim("Unit"))
                    WHEN 'un' THEN 'Unit'
                    WHEN 'unidade' THEN 'Unit'
                    WHEN 'unidades' THEN 'Unit'
                    WHEN 'g' THEN 'Gram'
                    WHEN 'grama' THEN 'Gram'
                    WHEN 'gramas' THEN 'Gram'
                    WHEN 'kg' THEN 'Kilogram'
                    WHEN 'ml' THEN 'Milliliter'
                    WHEN 'l' THEN 'Liter'
                    WHEN 'litro' THEN 'Liter'
                    WHEN 'litros' THEN 'Liter'
                    WHEN 'xícara' THEN 'Cup'
                    WHEN 'xícaras' THEN 'Cup'
                    WHEN 'xicara' THEN 'Cup'
                    WHEN 'xicaras' THEN 'Cup'
                    WHEN 'colher de sopa' THEN 'Tablespoon'
                    WHEN 'colheres de sopa' THEN 'Tablespoon'
                    WHEN 'colher de chá' THEN 'Teaspoon'
                    WHEN 'colheres de chá' THEN 'Teaspoon'
                    WHEN 'pitada' THEN 'Pinch'
                    ELSE "Unit"
                END;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecipeIngredients_Unit",
                table: "RecipeIngredients",
                sql: "\"Unit\" IN ('Unit', 'Gram', 'Kilogram', 'Milliliter', 'Liter', 'Cup', 'Tablespoon', 'Teaspoon', 'Pinch')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RecipeIngredients_Unit",
                table: "RecipeIngredients");

            migrationBuilder.Sql("""
                UPDATE "RecipeIngredients" SET "Unit" = CASE "Unit"
                    WHEN 'Unit' THEN 'un'
                    WHEN 'Gram' THEN 'g'
                    WHEN 'Kilogram' THEN 'kg'
                    WHEN 'Milliliter' THEN 'ml'
                    WHEN 'Liter' THEN 'l'
                    WHEN 'Cup' THEN 'xícara'
                    WHEN 'Tablespoon' THEN 'colher de sopa'
                    WHEN 'Teaspoon' THEN 'colher de chá'
                    WHEN 'Pinch' THEN 'pitada'
                    ELSE "Unit"
                END;
                """);
        }
    }
}
