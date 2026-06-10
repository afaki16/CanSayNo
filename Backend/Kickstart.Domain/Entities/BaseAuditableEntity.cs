using System;

namespace Kickstart.Domain.Entities
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
} 
