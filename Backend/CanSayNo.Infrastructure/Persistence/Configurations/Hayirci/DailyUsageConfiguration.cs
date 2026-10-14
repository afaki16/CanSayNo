using CanSayNo.Domain.Entities.Hayirci;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

public class DailyUsageConfiguration : IEntityTypeConfiguration<DailyUsage>
{
    public void Configure(EntityTypeBuilder<DailyUsage> builder)
    {
        builder.ToTable("DailyUsage");

        // Miras alınan int Id kullanılmıyor; composite PK (UserId, Date).
        builder.Ignore(du => du.Id);
        builder.HasKey(du => new { du.UserId, du.Date });

        builder.HasOne(du => du.User)
            .WithMany()
            .HasForeignKey(du => du.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(du => du.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(du => du.Date);
    }
}
