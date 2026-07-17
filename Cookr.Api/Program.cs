using Serilog;

var builder = WebApplication.CreateBuilder(args);

//  Serilog
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));


var app = builder.Build();
app.MapGet("/", () => Results.Ok(new { message = "Cookr is running!" }));
app.Run();