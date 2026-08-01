using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile(
        "appsettings.json",
        optional: true,
        reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddControllers();

var configuration = builder.Configuration;

builder.Services.AddInfrastructure(configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "postgresql");

var app = builder.Build();

var applyMigrations =
    configuration.GetValue<bool>("Database:ApplyMigrations");

if (applyMigrations)
{
    await using var scope = app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
