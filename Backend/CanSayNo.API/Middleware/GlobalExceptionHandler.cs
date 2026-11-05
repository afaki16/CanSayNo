using CanSayNo.Application.Common.Localization;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace CanSayNo.API.Middleware
{
    /// <summary>
    /// Catches any unhandled exception that escapes the MediatR pipeline / Result pattern.
    /// Logs full detail server-side and returns a generic error to the client.
    /// In Development the stack trace is included in the response to aid debugging.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IJsonStringLocalizer _localizer;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment,
            IJsonStringLocalizer localizer)
        {
            _logger = logger;
            _environment = environment;
            _localizer = localizer;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Cancellations are normal (user closed tab, request aborted) - don't log as errors.
            if (exception is OperationCanceledException)
                return false;

            var correlationId = httpContext.TraceIdentifier;

            // Unique index race: handlers check for duplicates first, but two concurrent
            // requests can both pass the check. The DB index rejects the second insert;
            // answer 409 like the handler would instead of a 500.
            var conflictMessageKey = GetUniqueViolationMessageKey(exception);
            if (conflictMessageKey != null)
            {
                _logger.LogWarning("Unique index violation mapped to 409. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId, httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = new
                    {
                        status = 409,
                        code = 1003, // ErrorCode.AlreadyExists
                        correlationId,
                        errors = new { error = new[] { _localizer[conflictMessageKey] } }
                    }
                }, cancellationToken);
                return true;
            }

            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId,
                httpContext.Request.Path,
                httpContext.Request.Method);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            // Match the response shape used by BaseController.HandleResult so the frontend
            // can rely on a single error contract: { success, error: { status, code, errors } }.
            var payload = new
            {
                success = false,
                error = new
                {
                    status = 500,
                    code = 1009, // ErrorCode.InternalError
                    correlationId,
                    errors = new
                    {
                        error = new[]
                        {
                            _localizer["common.unexpectedError"]
                        }
                    },
                    // Stack trace only in Development - never sent to production clients.
                    details = _environment.IsDevelopment() ? exception.ToString() : null
                }
            };

            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }

        private static string? GetUniqueViolationMessageKey(Exception exception)
        {
            for (var e = exception; e != null; e = e.InnerException)
            {
                if (e is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
                {
                    return pg.ConstraintName switch
                    {
                        "IX_Users_Email" => "users.emailExists",
                        "IX_Users_PhoneNumber" or "IX_Users_TenantId_PhoneNumber" => "users.phoneExists",
                        _ => null
                    };
                }
            }
            return null;
        }
    }
}