using CanSayNo.Domain.Entities.Hayirci;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        // Guid PK (BaseEntity.int Id gizlendi)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Status).HasConversion<int>();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.RawWebhookData).HasColumnType("jsonb");

        builder.Property(s => s.ProductId).HasMaxLength(100);
        builder.Property(s => s.Provider).HasMaxLength(50);
        builder.Property(s => s.ProviderUserId).HasMaxLength(200);
        builder.Property(s => s.ProviderTransactionId).HasMaxLength(200);
        builder.Property(s => s.AppId).HasMaxLength(50);

        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => new { s.UserId, s.AppId });
    }
}
