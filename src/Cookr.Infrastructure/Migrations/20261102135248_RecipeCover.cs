using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeCover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverFileName",
                table: "Recipes",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverFileName",
                table: "Recipes");
        }
    }
}
