using CanSayNo.Domain.Entities.Hayirci;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

public class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
{
    public void Configure(EntityTypeBuilder<UserStats> builder)
    {
        builder.ToTable("UserStats");

        // Miras alınan int Id kullanılmıyor; PK = UserId (User ile 1-1).
        builder.Ignore(us => us.Id);
        builder.HasKey(us => us.UserId);
        builder.Property(us => us.UserId).ValueGeneratedNever();

        builder.HasOne(us => us.User)
            .WithOne()
            .HasForeignKey<UserStats>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Badges as JSONB array
        builder.Property(us => us.UnlockedBadges).HasJsonbConversion("unlocked_badges");

        builder.Property(us => us.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}
