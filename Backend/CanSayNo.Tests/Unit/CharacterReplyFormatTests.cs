using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Tests.Unit;

public class CharacterReplyFormatTests
{
    [Fact]
    public void Parses_messages_and_mood()
    {
        var parsed = CharacterReplyFormat.Parse("""{"messages":["Bak şimdi…","Sadece 5 dakika."],"mood":"hurt"}""");

        Assert.NotNull(parsed);
        Assert.Equal(["Bak şimdi…", "Sadece 5 dakika."], parsed.Parts);
        Assert.Equal(CharacterMood.Hurt, parsed.Mood);
    }

    [Fact]
    public void Drops_blank_messages_and_trims()
    {
        var parsed = CharacterReplyFormat.Parse("""{"messages":["  Tamam. ","", "   "],"mood":"yielding"}""");

        Assert.NotNull(parsed);
        Assert.Equal(["Tamam."], parsed.Parts);
    }

    [Fact]
    public void More_than_three_messages_are_merged_into_the_last_one()
    {
        var parsed = CharacterReplyFormat.Parse("""{"messages":["Bir.","İki.","Üç.","Dört."],"mood":"insistent"}""");

        Assert.NotNull(parsed);
        Assert.Equal(["Bir.", "İki.", "Üç. Dört."], parsed.Parts);
    }

    [Theory]
    [InlineData("""{"messages":[],"mood":"calm"}""")]
    [InlineData("""{"messages":["",""],"mood":"calm"}""")]
    [InlineData("""{"mood":"calm"}""")]
    [InlineData("not json")]
    [InlineData("""{"messages":["yarım""")]
    public void Unusable_reply_returns_null(string json)
    {
        Assert.Null(CharacterReplyFormat.Parse(json));
    }

    [Fact]
    public void Unknown_mood_falls_back_to_insistent()
    {
        var parsed = CharacterReplyFormat.Parse("""{"messages":["Hadi ama."],"mood":"sneaky"}""");

        Assert.Equal(CharacterMood.Insistent, parsed!.Mood);
    }

    [Fact]
    public void Schema_lists_every_mood()
    {
        var schema = CharacterReplyFormat.BuildSchema();
        var moods = schema["properties"].GetProperty("mood").GetProperty("enum")
            .EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(Enum.GetValues<CharacterMood>().Length, moods.Count);
        Assert.Equal(["messages", "mood"], schema["required"].EnumerateArray().Select(e => e.GetString()));
    }
}
