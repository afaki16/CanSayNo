using Microsoft.AspNetCore.Mvc;

namespace Kickstart.Application.Common.Results;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        if (result.Error != null && result.Error.Errors.ContainsKey("NotFound"))
        {
            return new NotFoundObjectResult(result);
        }

        return new BadRequestObjectResult(result);
    }

    public static IActionResult ToActionResult<T>(this PagedResult<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        if (result.Error != null && result.Error.Errors.ContainsKey("NotFound"))
        {
            return new NotFoundObjectResult(result);
        }

        return new BadRequestObjectResult(result);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkResult();
        }

        if (result.Error != null && result.Error.Errors.ContainsKey("NotFound"))
        {
            return new NotFoundObjectResult(result);
        }

        return new BadRequestObjectResult(result);
    }
}
