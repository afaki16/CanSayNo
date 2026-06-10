using System.Text.Json.Serialization;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Common.Results;

public class ErrorResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; } = 400;

    [JsonPropertyName("code")]
    public ErrorCode Code { get; set; }

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public static ErrorResponse FromError(Error error)
    {
        return new ErrorResponse
        {
            Status = error.Status,
            Code = error.Code,
            Errors = error.ValidationErrors != null 
                ? new Dictionary<string, string[]>(error.ValidationErrors)
                : new Dictionary<string, string[]> { { "error", new[] { error.Message } } }
        };
    }

    public static ErrorResponse FromErrors(IEnumerable<Error> errors)
    {
        var firstError = errors.FirstOrDefault();
        if (firstError == null)
        {
            return new ErrorResponse();
        }

        var response = new ErrorResponse
        {
            Status = firstError.Status,
            Code = firstError.Code
        };

        // Combine all validation errors
        var allErrors = new Dictionary<string, string[]>();
        foreach (var error in errors)
        {
            if (error.ValidationErrors != null)
            {
                foreach (var kvp in error.ValidationErrors)
                {
                    if (allErrors.ContainsKey(kvp.Key))
                    {
                        allErrors[kvp.Key] = allErrors[kvp.Key].Concat(kvp.Value).ToArray();
                    }
                    else
                    {
                        allErrors[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                if (allErrors.ContainsKey("error"))
                {
                    allErrors["error"] = allErrors["error"].Concat(new[] { error.Message }).ToArray();
                }
                else
                {
                    allErrors["error"] = new[] { error.Message };
                }
            }
        }

        response.Errors = allErrors;
        return response;
    }
} 