using EasyPeasy.App.Services;

namespace EasyPeasy.App.Tests;

/// <summary>
/// Tests for <see cref="EntryTextRenderer"/> — how an entry is shown to the learner.
/// The markers that <see cref="AnswerMatcher"/> reads must never reach the screen as punctuation,
/// and the parts they mark must be visually quieter than the entry itself.
/// </summary>
public class EntryTextRendererTests
{
    [Theory]
    [InlineData("look [at] sb", "look <span class=\"entry-optional\">at</span> <span class=\"entry-placeholder\">sb</span>")]
    [InlineData("[really] look", "<span class=\"entry-optional\">really</span> look")]
    [InlineData("take [good] care [of it]",
        "take <span class=\"entry-optional\">good</span> care <span class=\"entry-optional\">of it</span>")]
    public void Optional_parts_lose_their_brackets_and_get_dimmed(string entry, string html)
    {
        Assert.Equal(html, EntryTextRenderer.ToHtml(entry));
    }

    [Theory]
    [InlineData("nobody does sth", "nobody does <span class=\"entry-placeholder\">sth</span>")]
    [InlineData("give sb sth",
        "give <span class=\"entry-placeholder\">sb</span> <span class=\"entry-placeholder\">sth</span>")]
    [InlineData("look after somebody's something",
        "look after <span class=\"entry-placeholder\">somebody's</span> <span class=\"entry-placeholder\">something</span>")]
    public void Placeholders_are_dimmed_even_without_brackets(string entry, string html)
    {
        // A slot word is not part of the phrase to learn, so it should not read as loud as one.
        Assert.Equal(html, EntryTextRenderer.ToHtml(entry));
    }

    [Fact]
    public void Ordinary_words_stay_untouched()
    {
        Assert.Equal("to break down", EntryTextRenderer.ToHtml("to break down"));
    }

    [Fact]
    public void Literal_braces_disappear_without_a_trace()
    {
        Assert.Equal("to and fro", EntryTextRenderer.ToHtml("{to} and fro"));
    }

    [Fact]
    public void Html_in_the_entry_is_escaped()
    {
        Assert.Equal("a &lt;b&gt; tag", EntryTextRenderer.ToHtml("a <b> tag"));
    }

    [Fact]
    public void Unclosed_bracket_is_shown_as_plain_text()
    {
        Assert.Equal("look [at", EntryTextRenderer.ToHtml("look [at"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_entry_renders_nothing(string? entry)
    {
        Assert.Equal("", EntryTextRenderer.ToHtml(entry));
    }
}
