using Kickstart.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kickstart.Application.Features.Permissions.Dtos
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Resource { get; set; }
        public PermissionType Type { get; set; }
        public string FullPermission { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> IndividualPermissions { get; set; } = new List<string>();

        // Flag enum için yardımcı metodlar
        public bool HasPermission(PermissionType permissionType) => 
            Type.HasFlag(permissionType);

        public bool HasAnyPermission(PermissionType permissionTypes) => 
            (Type & permissionTypes) != PermissionType.None;
    }
} 
