using AutoMapper;
using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Tenants.Queries.GetAllTenants;
using Kickstart.Application.Features.Tenants.Queries.GetTenantById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.API.Controllers;

[Authorize(Policy = "RequireSuperAdminRole")]
[ApiController]
[Route("api/[controller]")]
public class TenantsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public TenantsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all tenants with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering</param>
    /// <returns>List of tenants</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantListDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAllTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null)
    {
        var query = new GetAllTenantsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Tenant details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TenantDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTenantById(int id)
    {
        var query = new GetTenantByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    /// <param name="dto">Tenant creation data</param>
    /// <returns>Created tenant</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TenantListDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
    {
        var command = _mapper.Map<CreateTenantCommand>(dto);

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetTenantById), new { id = result.Value.Id },
                new { success = true, data = result.Value });

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <param name="dto">Tenant update data</param>
    /// <returns>Updated tenant</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TenantListDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTenant(int id, [FromBody] UpdateTenantDto dto)
    {
        var command = _mapper.Map<UpdateTenantCommand>(dto);
        command.Id = id;

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a tenant (soft delete)
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTenant(int id)
    {
        var command = new DeleteTenantCommand { Id = id };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
