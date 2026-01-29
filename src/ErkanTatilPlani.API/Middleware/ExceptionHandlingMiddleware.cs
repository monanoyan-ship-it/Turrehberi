using System.Net;
using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Services;

namespace ErkanTatilPlani.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Tum hatalari yakalar ve uygun JSON response doner
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        // Hata tipine gore status code ve mesaj belirle
        var (statusCode, title, detail) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Gecersiz istek", "Zorunlu bir parametre eksik."),
            ArgumentException => (HttpStatusCode.BadRequest, "Gecersiz parametre", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Bulunamadi", "Istenen kaynak bulunamadi."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erisim", "Bu islemi yapmak icin yetkiniz yok."),
            InvalidOperationException => (HttpStatusCode.Conflict, "Gecersiz islem", exception.Message),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Desteklenmiyor", "Bu ozellik henuz desteklenmiyor."),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "Zaman asimi", "Islem zaman asimina ugradi."),
            _ => (HttpStatusCode.InternalServerError, "Sunucu hatasi", "Beklenmeyen bir hata olustu.")
        };

        // Hatayi logla (console)
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Beklenmeyen hata olustu. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Istek hatasi. StatusCode: {StatusCode}, TraceId: {TraceId}", (int)statusCode, traceId);
        }

        // Veritabanina logla
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetService<IAppLogService>();
            if (logService != null)
            {
                var log = new AppLog
                {
                    Level = statusCode == HttpStatusCode.InternalServerError ? LogLevels.Error : LogLevels.Warning,
                    Message = title,
                    Exception = exception.Message,
                    StackTrace = exception.StackTrace,
                    Source = "ExceptionMiddleware",
                    TraceId = traceId,
                    StatusCode = (int)statusCode,
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault()
                };

                // Kullanici bilgileri
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    log.UserId = context.User.FindFirst("sub")?.Value;
                    log.UserName = context.User.FindFirst("email")?.Value;
                }

                await logService.LogAsync(log);
            }
        }
        catch
        {
            // Loglama hatasi - sessizce gec
        }

        // Response olustur
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = _env.IsDevelopment() ? exception.Message : detail,
            traceId,
            // Development'ta stack trace ekle
            stackTrace = _env.IsDevelopment() ? exception.StackTrace : null,
            innerException = _env.IsDevelopment() && exception.InnerException != null
                ? exception.InnerException.Message
                : null
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}

/// <summary>
/// Middleware extension metodu
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
