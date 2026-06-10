using System.Text.Json.Serialization;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Common.Results;

public class PagedResult<T> : Result
{
    private PagedResult(IEnumerable<T> items, int pageNumber, int totalPages, int totalItems, bool isSuccess,
        IEnumerable<Error> errors)
        : base(isSuccess, errors)
    {
        Items = items;
        PageNumber = pageNumber;
        TotalPages = totalPages;
        TotalItems = totalItems;
    }

    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResult<T> Success(IEnumerable<T> items, int pageNumber, int totalPages, int totalCount)
        => new(items, pageNumber, totalPages, totalCount, true, []);

    public static new PagedResult<T> Failure(params Error[] errors)
        => new([], 1, 0, 0, false, errors);

    public static new PagedResult<T> Failure(IEnumerable<Error> errors)
        => new([], 1, 0, 0, false, errors);

    public TResult Match<TResult>(
        Func<IEnumerable<T>, int, int, int, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure) =>
        IsSuccess ? onSuccess(Items, PageNumber, TotalPages, TotalItems) : onFailure(Errors);
}
