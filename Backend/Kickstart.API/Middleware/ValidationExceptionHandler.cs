using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Kickstart.API.Middleware
{
    /// <summary>
    /// Catches FluentValidation.ValidationException thrown by the MediatR ValidationBehavior
    /// (or anywhere else in the pipeline) and turns it into a 400 response with
    /// field-by-field error details, matching the shape used by BaseController.HandleResult.
    /// Must be registered BEFORE GlobalExceptionHandler so it gets first dibs on the exception.
    /// </summary>
    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException validationException)
                return false; // not our problem - let the next handler try

            _logger.LogWarning(
                "Validation failed. Path: {Path}, ErrorCount: {ErrorCount}",
                httpContext.Request.Path,
                validationException.Errors.Count());

            // Group errors by PropertyName so the client gets one entry per field with all
            // its messages: { "Email": ["required", "invalid format"], "Password": ["too short"] }
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                error = new
                {
                    status = 400,
                    code = 1001, // ErrorCode.ValidationFailed
                    errors
                }
            };

            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }
    }
}