using Cookr.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cookr.Infrastructure;

public class CookrDbContext(DbContextOptions<CookrDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.Property(r => r.Title).HasMaxLength(Recipe.TitleMaxLength);
            entity.Property(r => r.Instructions).HasMaxLength(Recipe.InstructionsMaxLength);
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.Property(i => i.Name)
                .HasMaxLength(Ingredient.NameMaxLength)
                .UseCollation("NOCASE");
            entity.HasIndex(i => i.Name).IsUnique();
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(ri => new { ri.RecipeId, ri.IngredientId });
            entity.Property(ri => ri.Unit).HasMaxLength(RecipeIngredient.UnitMaxLength);

            entity.HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            entity.HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
