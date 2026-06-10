using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Roles.Dtos
{
    public class UpdateRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 
