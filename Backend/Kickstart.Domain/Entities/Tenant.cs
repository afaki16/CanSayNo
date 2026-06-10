using Kickstart.Domain.Entities;
using System.Collections.Generic;

namespace Kickstart.Domain.Entities;

    public class Tenant : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Domain { get; set; }
    public bool IsActive { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; }

    public Tenant()
    {
        Users = new HashSet<User>();
        IsActive = true;
    }
}
