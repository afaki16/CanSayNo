using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Domain)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Domain)
            .IsUnique()
            .HasFilter("\"Domain\" IS NOT NULL");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(100);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(20);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(x => x.Users)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.SetNull); // Tenant silinirse User'ların TenantId'si null olur
    }
}
}