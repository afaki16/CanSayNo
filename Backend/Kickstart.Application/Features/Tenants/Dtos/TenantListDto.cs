using System;

namespace Kickstart.Application.Features.Tenants.Dtos
{
    public class TenantListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public bool IsActive { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserCount { get; set; }
    }
}
