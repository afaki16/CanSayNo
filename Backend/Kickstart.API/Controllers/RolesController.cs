using Kickstart.API.Controllers;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Roles.Commands.CreateRole;
using Kickstart.Application.Features.Roles.Commands.UpdateRole;
using Kickstart.Application.Features.Roles.Commands.DeleteRole;
using Kickstart.Application.Features.Roles.Queries.GetAllRoles;
using Kickstart.Application.Features.Roles.Queries.GetRoleById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.API.Controllers
{
    [Authorize]
    public class RolesController : BaseController
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all roles with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of roles</returns>
        [HttpGet]
        [Authorize(Policy = "roles.read")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = null)
        {
            var query = new GetAllRolesQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);
            return HandlePagedResult(result);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Role details</returns>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "roles.read")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var query = new GetRoleByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="dto">Role creation data</param>
        /// <returns>Created role</returns>
        [HttpPost]
        [Authorize(Policy = "roles.create")]
        [ProducesResponseType(typeof(RoleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var command = new CreateRoleCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                PermissionIds = dto.PermissionIds
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetRoleById), new { id = result.Value.Id }, new { success = true, data = result.Value });
            
            return HandleResult(result);
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="dto">Role update data</param>
        /// <returns>Updated role</returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "roles.update")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var command = new UpdateRoleCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                PermissionIds = dto.PermissionIds
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "roles.delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var command = new DeleteRoleCommand { Id = id };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
