using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.ErrorHandling;

public sealed class GlobalExceptionHandler :
    IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException =>
                StatusCodes.Status400BadRequest,

            UnauthorizedException =>
                StatusCodes.Status401Unauthorized,

            NotFoundException =>
                StatusCodes.Status404NotFound,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Ocurrió una excepción no controlada.");
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                400 => "Error de validación",
                401 => "Error de autenticación",
                404 => "Recurso no encontrado",
                409 => "Conflicto con el estado actual del recurso.",
                _ => "Error interno del servidor"
            },
            Detail = statusCode == 500
                ? "Ocurrió un error inesperado."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
