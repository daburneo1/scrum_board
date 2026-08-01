using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using WebApi.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["Jwt:Key"] =
    Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException(
        "La variable de entorno JWT_KEY no está configurada.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var (title, detail) = context.ProblemDetails.Status switch
        {
            StatusCodes.Status400BadRequest =>
                ("Solicitud incorrecta", "La solicitud contiene datos inválidos."),
            StatusCodes.Status401Unauthorized =>
                ("No autorizado", "Se requiere autenticación para acceder al recurso."),
            StatusCodes.Status403Forbidden =>
                ("Acceso prohibido", "No tiene permisos para acceder al recurso."),
            StatusCodes.Status404NotFound =>
                ("Recurso no encontrado", "El recurso solicitado no existe."),
            StatusCodes.Status405MethodNotAllowed =>
                ("Método no permitido", "El método HTTP no está permitido para este recurso."),
            StatusCodes.Status409Conflict =>
                ("Conflicto", "La solicitud entra en conflicto con el estado actual del recurso."),
            StatusCodes.Status415UnsupportedMediaType =>
                ("Tipo de contenido no compatible", "El tipo de contenido enviado no es compatible."),
            StatusCodes.Status429TooManyRequests =>
                ("Demasiadas solicitudes", "Se realizaron demasiadas solicitudes."),
            _ when context.ProblemDetails.Status is >= 400 and < 500 =>
                ("Error en la solicitud", "No se pudo procesar la solicitud."),
            _ =>
                ("Error interno del servidor", "Ocurrió un error inesperado.")
        };

        context.ProblemDetails.Title = title;
        context.ProblemDetails.Detail = detail;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("postgresql");

var app = builder.Build();

var applyMigrations =
    app.Configuration.GetValue<bool>(
        "Database:ApplyMigrations");

if (applyMigrations)
{
    await using var scope =
        app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
