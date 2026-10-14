using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CanSayNo.Infrastructure.Persistence.Configurations.Hayirci;

/// <summary>
/// Karmaşık (owned olmayan) CLR tiplerini PostgreSQL <c>jsonb</c> kolonlarına serialize eden
/// yardımcı. EF Core owned-JSON (<c>.ToJson()</c>) yerine System.Text.Json + ValueConverter
/// kullanır; böylece <c>Dictionary&lt;enum,int&gt;</c> ve <c>List&lt;enum&gt;</c> gibi
/// owned-JSON'un zorlandığı yapılar sorunsuz map'lenir.
///
/// Ayrıca bir <see cref="ValueComparer{T}"/> bağlar; aksi halde EF değişiklik takibini
/// referansla yapar ve liste/sözlük içi mutasyonları kaçırır (ve model-build uyarısı verir).
/// </summary>
internal static class JsonbColumnExtensions
{
    // Enum'lar sayısal değerleriyle serialize edilir (round-trip güvenli, kompakt).
    private static readonly JsonSerializerOptions Options = new();

    public static PropertyBuilder<T> HasJsonbConversion<T>(
        this PropertyBuilder<T> builder, string columnName)
    {
        var converter = new ValueConverter<T, string>(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<T>(v, Options)!);

        var comparer = new ValueComparer<T>(
            (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
            v => v == null ? 0 : JsonSerializer.Serialize(v, Options).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, Options), Options)!);

        builder
            .HasColumnName(columnName)
            .HasColumnType("jsonb")
            .HasConversion(converter, comparer);

        return builder;
    }
}
