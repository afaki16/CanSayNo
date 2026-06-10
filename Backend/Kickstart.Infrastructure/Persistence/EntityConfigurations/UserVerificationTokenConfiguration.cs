using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations
{
    public class UserVerificationTokenConfiguration : IEntityTypeConfiguration<UserVerificationToken>
    {
        public void Configure(EntityTypeBuilder<UserVerificationToken> builder)
        {
            builder.ToTable("UserVerificationTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Purpose)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Destination)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.UsedAt)
                .IsRequired(false);

            builder.Property(x => x.RequestIpAddress)
                .HasMaxLength(45);

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(x => x.Token);

            builder.HasIndex(x => new { x.UserId, x.Channel, x.Purpose, x.IsUsed, x.ExpiresAt })
                .HasDatabaseName("IX_UserVerificationTokens_User_Channel_Purpose_IsUsed_ExpiresAt");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
