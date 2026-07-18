using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services.AddDbContext<CookrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CookrDb")));

builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var version = typeof(Program).Assembly.GetName().Version?.ToString(3);
app.MapGet("/", () => Results.Ok(new { message = "Cookr is running!", version }));

app.MapRecipeEndpoints();
app.MapIngredientEndpoints();

app.Run();