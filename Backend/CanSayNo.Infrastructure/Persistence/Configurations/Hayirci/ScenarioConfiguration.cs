using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

public class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
{
    public void Configure(EntityTypeBuilder<Scenario> builder)
    {
        builder.ToTable("Scenarios");

        // string PK (BaseEntity.int Id gizlendi)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasMaxLength(100);

        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
        builder.Property(s => s.ShortDescription).HasMaxLength(500);

        builder.Property(s => s.ContextId).HasConversion<int>();

        // İçerik dili; mevcut satırlar migration'da 'tr' alır
        builder.Property(s => s.Language).HasMaxLength(5).IsRequired().HasDefaultValue(ContentLanguages.Turkish);

        // JSONB kolonları (System.Text.Json üzerinden)
        builder.Property(s => s.Scene).HasJsonbConversion("scene");
        builder.Property(s => s.Character).HasJsonbConversion("character");
        builder.Property(s => s.PressureTactics).HasJsonbConversion("pressure_tactics");
        builder.Property(s => s.Tags).HasJsonbConversion("tags");

        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

        // Indexes
        builder.HasIndex(s => s.ContextId);
        builder.HasIndex(s => new { s.Language, s.ContextId });
        builder.HasIndex(s => s.IsFree);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.SortOrder);
    }
}
