using Cookr.Api.Features.Recipes;
using Cookr.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//  Serilog
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services.AddDbContext<CookrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CookrDb")));

builder.Services.AddScoped<IRecipeService, RecipeService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "Cookr is running!" }));
app.MapRecipeEndpoints();

app.Run();