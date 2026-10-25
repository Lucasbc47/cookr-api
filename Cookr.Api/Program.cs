using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Cookr.Api.Features.Ingredients;
using Cookr.Api.Features.Recipes;
using Cookr.Api.Shared;
using Cookr.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services.AddDbContext<CookrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CookrDb")));

builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (ctx, _, ex) =>
    {
        if (ex is not null || ctx.Response.StatusCode >= 500) return LogEventLevel.Error;
        if (ctx.Request.Path.StartsWithSegments("/scalar")) return LogEventLevel.Verbose;
        return LogEventLevel.Information;
    };
});

app.UseCors();

if (app.Environment.IsDevelopment())
{
    await app.MigrateAndSeedAsync();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var version = typeof(Program).Assembly.GetName().Version?.ToString(3);
app.MapGet("/", () => Results.Ok(new { message = "Cookr is running!", version }));

app.MapRecipeEndpoints();
app.MapIngredientEndpoints();

await app.RunAsync();
