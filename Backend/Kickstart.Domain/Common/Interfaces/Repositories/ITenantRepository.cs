using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories;

public interface ITenantRepository : IRepository<Tenant, int>
{
    Task<Tenant> GetByNameAsync(string name);
    Task<Tenant> GetByDomainAsync(string domain);
    Task<Tenant> GetTenantWithUsersAsync(int tenantId);

    /// <summary>
    /// Read-only variant of GetTenantWithUsersAsync - returns the tenant with no change tracking.
    /// Use this from Query handlers; use GetTenantWithUsersAsync from Command handlers that
    /// will modify the returned entity.
    /// </summary>
    Task<Tenant> GetTenantWithUsersReadOnlyAsync(int tenantId);
    Task<bool> NameExistsAsync(string name);
    Task<bool> DomainExistsAsync(string domain);
}

