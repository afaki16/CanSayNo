using System.Text.RegularExpressions;
using CanSayNo.Application.Features.Admin.Hayirci.Scenarios;
using CanSayNo.Domain.Enums;
using CanSayNo.Infrastructure.Persistence.Seeding;

namespace CanSayNo.Tests.Unit;

/// <summary>
/// Seed senaryolarının içerik kuralları: admin panelinde düzenlenebilsinler (aynı sınırlar),
/// ID dil/bağlam önekine uysun, ücretsiz katman yalnızca İş bağlamı olsun, sayılar beklendiği gibi.
/// </summary>
public class SeedScenarioTests
{
    private static readonly List<Domain.Entities.Hayirci.Scenario> All = HayirciSeedData.GetSeedScenarios();
    private static readonly Regex IdPattern =
        new("^(en_)?(is|aile|arkadas|romantik|satici|komsu|okul|dijital)_[0-9]{3}_[a-z0-9_]{2,60}$");

    [Fact]
    public void Ids_are_unique() =>
        Assert.Empty(All.GroupBy(s => s.Id).Where(g => g.Count() > 1).Select(g => g.Key));

    [Fact]
    public void Counts_per_language_and_context()
    {
        var tr = All.Where(s => s.Language == ContentLanguages.Turkish).ToList();
        var en = All.Where(s => s.Language == ContentLanguages.English).ToList();

        Assert.Equal(65, tr.Count);
        Assert.Equal(45, en.Count);
        Assert.Equal(10, en.Count(s => s.ContextId == ContextId.Is));
        foreach (var context in Enum.GetValues<ContextId>().Where(c => c != ContextId.Is))
            Assert.True(en.Count(s => s.ContextId == context) == 5, $"en {context}: {en.Count(s => s.ContextId == context)}");
    }

    [Fact]
    public void Only_work_context_is_free() =>
        Assert.All(All, s => Assert.Equal(s.ContextId == ContextId.Is, s.IsFree));

    [Fact]
    public void English_ids_match_language_and_context_prefix() =>
        Assert.All(All.Where(s => s.Language == ContentLanguages.English), s =>
        {
            Assert.Matches(IdPattern, s.Id);
            Assert.StartsWith(AdminScenarioIds.Prefix(s.ContextId, s.Language) + "_", s.Id);
        });

    [Fact]
    public void Turkish_ids_have_no_language_prefix() =>
        Assert.All(All.Where(s => s.Language == ContentLanguages.Turkish), s =>
            Assert.StartsWith(AdminScenarioIds.Prefix(s.ContextId) + "_", s.Id));

    [Fact]
    public void English_content_fits_admin_limits_and_is_complete() =>
        Assert.All(All.Where(s => s.Language == ContentLanguages.English), s =>
        {
            Assert.InRange(s.Title.Length, 1, 80);
            Assert.InRange(s.ShortDescription.Length, 1, 160);
            Assert.InRange(s.Difficulty, 1, 5);
            Assert.InRange(s.EstimatedDurationMinutes, 1, 30);
            Assert.InRange(s.Tags.Count, 1, 8);
            Assert.All(s.Tags, t => Assert.InRange(t.Length, 1, 30));
            Assert.False(string.IsNullOrWhiteSpace(s.Scene.Setting));
            Assert.False(string.IsNullOrWhiteSpace(s.Scene.Situation));
            Assert.False(string.IsNullOrWhiteSpace(s.Scene.UserGoal));
            Assert.False(string.IsNullOrWhiteSpace(s.Scene.Hint));
            Assert.False(string.IsNullOrWhiteSpace(s.Character.Name));
            Assert.NotEmpty(s.PressureTactics.RoundOne);
            Assert.NotEmpty(s.PressureTactics.RoundTwo);
            Assert.NotEmpty(s.PressureTactics.RoundThree);

            var words = s.Scene.OpeningMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.True(words is > 0 and <= 60, $"{s.Id}: opening {words} words");
        });

    [Fact]
    public void English_content_has_no_turkish_characters() =>
        Assert.All(All.Where(s => s.Language == ContentLanguages.English), s =>
            Assert.DoesNotMatch("[çğıöşüÇĞİÖŞÜ]",
                string.Join(" ", s.Title, s.ShortDescription, s.Scene.Setting, s.Scene.Situation,
                    s.Scene.UserGoal, s.Scene.Hint, s.Scene.OpeningMessage, s.Character.Name,
                    s.Character.Role, s.Character.Relationship)));
}
