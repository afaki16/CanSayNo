using System.Text.Json.Serialization;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Common.Results;

public class Result<T> : Result
{
    private Result(T value, bool isSuccess, IEnumerable<Error> errors)
        : base(isSuccess, errors) =>
        Value = value;

    [JsonPropertyName("value")]
    public T Value { get; }

    public static Result<T> Success(T value) => new(value, true, []);

    public static new Result<T> Failure(params Error[] errors) => new(default!, false, errors);

    public static new Result<T> Failure(IEnumerable<Error> errors) => new(default!, false, errors);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Errors);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
