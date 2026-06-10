using System.Text.Json.Serialization;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Common.Results;

public class Result
{
    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        List<Error> errorList = errors.ToList();
        bool hasErrors = errorList.Count > 0;
        switch (isSuccess)
        {
            case true when hasErrors:
                throw new InvalidOperationException("A successful result cannot have errors.");
            case false when !hasErrors:
                throw new InvalidOperationException("A failure result must contain at least one error.");
            default:
                IsSuccess = isSuccess;
                Errors = errorList.AsReadOnly();
                break;
        }
    }

    [JsonIgnore]
    internal IReadOnlyList<Error> Errors { get; }

    [JsonPropertyName("isFailure")]
    public bool IsFailure => !IsSuccess;

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; }

    [JsonPropertyName("error")]
    public ErrorResponse? Error => IsFailure ? ErrorResponse.FromErrors(Errors) : null;

    public static Result Success() => new(true, []);

    public static Result Failure(params Error[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

}
