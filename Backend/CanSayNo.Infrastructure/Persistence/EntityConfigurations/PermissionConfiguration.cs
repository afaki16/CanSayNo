using CanSayNo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.EntityConfigurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Resource)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // No unique index on Resource + Type: non-CRUD actions (e.g. "Invoices.Export") have no
            // matching PermissionType and fall back to Read, which would collide with "Invoices.Read"
            // and crash startup seeding. Name is the unique identifier (see IX_Permissions_Name).

            // Relationships
            builder.HasMany(x => x.RolePermissions)
                .WithOne(x => x.Permission)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 
