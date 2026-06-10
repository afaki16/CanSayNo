
namespace Kickstart.Application.Features.Tenants.Dtos
{
    public class UpdateTenantDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public bool IsActive { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }
}
