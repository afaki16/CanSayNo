using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiryDate })
                .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiryDate");

            builder.Property(x => x.ExpiryDate)
                .IsRequired();

            builder.Property(x => x.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships are configured in User configuration
        }
    }
} 
