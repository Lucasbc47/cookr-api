using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeTitleUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Recipes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Title",
                table: "Recipes",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_Title",
                table: "Recipes");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Recipes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldCollation: "NOCASE");
        }
    }
}
