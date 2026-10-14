using CanSayNo.Domain.Entities.Hayirci;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        // Guid PK (BaseEntity.int Id gizlendi)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Status).HasConversion<int>();

        // Relationships
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Scenario)
            .WithMany(sc => sc.Sessions)
            .HasForeignKey(s => s.ScenarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // JSONB kolonları
        builder.Property(s => s.Messages).HasJsonbConversion("messages");
        builder.Property(s => s.Evaluation).HasJsonbConversion("evaluation");

        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ScenarioId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.StartedAt);
    }
}
