using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Tenants.Dtos
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public bool IsActive { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int UserCount { get; set; }
    }
}
