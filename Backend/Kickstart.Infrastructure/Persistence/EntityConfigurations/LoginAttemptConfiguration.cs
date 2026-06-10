using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations
{
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IpAddress)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(50);

            builder.Property(x => x.AttemptedAt)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(x => new { x.Email, x.IpAddress, x.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Email_IpAddress_AttemptedAt");

            builder.HasIndex(x => new { x.Email, x.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Email_AttemptedAt");
        }
    }
}
