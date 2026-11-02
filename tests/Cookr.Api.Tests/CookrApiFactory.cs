using System.Net.Http.Json;
using System.Text.Json;
using Cookr.Api.Shared;
using Cookr.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cookr.Api.Tests;

internal class CookrApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public string CoversPath { get; } = Path.Combine(Path.GetTempPath(), "cookr-tests", Guid.NewGuid().ToString("N"));

    public static JsonSerializerOptions Json { get; } = CreateJsonOptions();

    public static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        return problem.GetProperty("code").GetString();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        JsonConfig.Apply(options);
        return options;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("RecipeCovers:Path", CoversPath);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CookrDbContext>>();

            _connection.Open();
            services.AddDbContext<CookrDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<CookrDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
        if (Directory.Exists(CoversPath)) Directory.Delete(CoversPath, recursive: true);
    }
}
