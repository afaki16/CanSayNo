using Kickstart.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Kickstart.Application.Common.Results;

namespace Kickstart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(new { success = true, data = result.Value });
        }

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true, message = "Operation completed successfully" });

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected IActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    items = result.Items,
                    totalCount = result.TotalItems,
                    totalPages = result.TotalPages,
                    pageNumber = result.PageNumber
                }
            });
        }

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected string GetIpAddress()
    {
        // UseForwardedHeaders middleware X-Forwarded-For'u zaten consume edip
        // Connection.RemoteIpAddress'e yazıyor (KnownNetworks filtresi ile).
        // Manuel header okumak bu güvenlik filtresini bypass eder ve spoofing'e
        // yol açar — saldırgan direkt API'ye vurursa kendi IP'sini gizleyebilir
        // veya başka kullanıcıların IP'sini lockout'a sokabilir.
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip == null) return "Unknown";

        // IPv6 loopback (::1) → 127.0.0.1
        if (ip.Equals(System.Net.IPAddress.IPv6Loopback))
            return "127.0.0.1";

        // IPv4-mapped IPv6 (::ffff:x.x.x.x) → x.x.x.x
        if (ip.IsIPv4MappedToIPv6)
            return ip.MapToIPv4().ToString();

        return ip.ToString();
    }

    protected string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
