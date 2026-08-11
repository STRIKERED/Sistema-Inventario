using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Middleware;

/// <summary>
/// Red de seguridad global: convierte excepciones no manejadas en respuestas ProblemDetails con el
/// status code adecuado, en vez de dejar que ASP.NET Core devuelva un 500 genérico (o el stack trace
/// en desarrollo). Los controllers que ya capturan excepciones puntuales (p. ej. para dar un mensaje
/// más específico) siguen haciéndolo; este middleware solo actúa sobre lo que se les escapa.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, titulo) = ClasificarExcepcion(ex);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "Error no controlado procesando {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex, "{Titulo} en {Method} {Path}", titulo, context.Request.Method, context.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            // El mensaje de la excepción es seguro de mostrar aquí: los casos 4xx son errores de
            // negocio/validación con mensajes redactados a propósito para el usuario final. Los 500
            // ocultan el detalle real y devuelven un mensaje genérico.
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado al procesar la solicitud."
                : ex.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (int StatusCode, string Titulo) ClasificarExcepcion(Exception ex) => ex switch
    {
        ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Parámetro fuera de rango"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
    };
}
