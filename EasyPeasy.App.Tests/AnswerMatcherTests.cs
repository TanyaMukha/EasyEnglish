using EasyPeasy.App.Services;

namespace EasyPeasy.App.Tests;

/// <summary>
/// Tests for <see cref="AnswerMatcher"/> — what counts as a correct typed answer.
/// This is the highest-consequence string logic in the app: a false negative tells a learner who
/// knows the word that they got it wrong (and pushes the word's difficulty up), while a false
/// positive quietly marks an unknown word as learned.
/// </summary>
public class AnswerMatcherTests
{
    // ── Exact and near-exact answers ──────────────────────────────────────────

    [Theory]
    [InlineData("cat", "cat")]
    [InlineData("cat", "Cat")]
    [InlineData("cat", "  CAT  ")]
    [InlineData("give up", "give   up")]
    [InlineData("don't", "don’t")]          // typographic apostrophe
    [InlineData("cat", "cat.")]             // trailing punctuation
    public void Accepts_equivalent_spellings(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("cat", "dog")]
    [InlineData("give up", "give")]
    [InlineData("give up", "give up now")]
    [InlineData("cat", "")]
    public void Rejects_different_answers(string expected, string typed)
    {
        Assert.False(AnswerMatcher.Matches(expected, typed));
    }

    // ── Articles ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("a cat", "a cat")]
    [InlineData("a cat", "cat")]
    [InlineData("an apple", "apple")]
    [InlineData("to be a doctor", "be doctor")]
    public void Indefinite_articles_are_optional(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("the same", "same")]
    [InlineData("the same", "the same")]
    public void Leading_definite_article_is_optional(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("cat", "a cat")]
    [InlineData("cat", "the cat")]
    [InlineData("look at sb", "to look at sb")]
    [InlineData("take care of sb", "take good care of sb")]
    public void Words_the_entry_does_not_have_are_rejected(string expected, string typed)
    {
        // The permission is one-way: optional parts may be dropped, nothing may be added.
        // A word does not automatically take an article, so "a cat" is not an answer for "cat".
        Assert.False(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void Definite_article_inside_a_phrase_is_required()
    {
        // "in end" is not English — only the leading article is treated as noise.
        Assert.False(AnswerMatcher.Matches("in the end", "in end"));
        Assert.True(AnswerMatcher.Matches("in the end", "in the end"));
    }

    // ── Infinitive marker ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("to look", "look")]
    [InlineData("to look", "to look")]
    public void Leading_to_is_optional(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void To_inside_a_phrase_is_required()
    {
        Assert.True(AnswerMatcher.Matches("to go to school", "go to school"));
        Assert.True(AnswerMatcher.Matches("to go to school", "to go to school"));
        Assert.False(AnswerMatcher.Matches("to go to school", "go school"));
    }

    [Fact]
    public void Braces_keep_a_leading_word_required()
    {
        // The escape hatch for a leading word that belongs to the entry: this "to" is part of
        // the idiom, and "a few" is not the word "few".
        Assert.True(AnswerMatcher.Matches("{to} and fro", "to and fro"));
        Assert.False(AnswerMatcher.Matches("{to} and fro", "and fro"));
        Assert.False(AnswerMatcher.Matches("{a} few", "few"));
    }

    // ── Bracketed parts ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("look [at] sb", "look at sb")]
    [InlineData("look [at] sb", "look sb")]
    public void Bracketed_group_may_be_typed_or_left_out(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("take [good] care [of sb]", "take good care of sb")]   // 1 1
    [InlineData("take [good] care [of sb]", "take good care")]         // 1 0
    [InlineData("take [good] care [of sb]", "take care of sb")]        // 0 1
    [InlineData("take [good] care [of sb]", "take care")]              // 0 0
    public void Every_combination_of_two_groups_is_accepted(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void Three_groups_accept_all_eight_combinations()
    {
        const string expected = "[really] look [at] sb [now]";

        string[] accepted =
        [
            "really look at sb now", "really look at sb", "really look sb now", "really look sb",
            "look at sb now",        "look at sb",        "look sb now",        "look sb",
        ];

        Assert.All(accepted, typed => Assert.True(AnswerMatcher.Matches(expected, typed), typed));
    }

    [Fact]
    public void A_bracketed_group_is_all_or_nothing()
    {
        // "[of sb]" is one unit: half of it is not an answer.
        Assert.False(AnswerMatcher.Matches("take care [of sb]", "take care of"));
        Assert.False(AnswerMatcher.Matches("take care [of sb]", "take care sb"));
    }

    // ── Placeholders ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("give sb sth", "give somebody something")]
    [InlineData("give sb sth", "give someone sth")]
    [InlineData("give somebody something", "give sb sth")]
    [InlineData("give sb sth", "give smb smth")]
    [InlineData("look after sb's sth", "look after somebody's something")]
    public void Placeholders_may_be_written_in_full_or_short(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void Placeholders_are_not_interchangeable_with_each_other()
    {
        Assert.False(AnswerMatcher.Matches("give sb sth", "give sth sb"));
    }

    // ── Equivalent wordings ───────────────────────────────────────────────────

    [Theory]
    [InlineData("configuration / config", "configuration")]
    [InlineData("configuration / config", "config")]
    [InlineData("configuration/config", "config")]                  // no spaces around the slash
    [InlineData("to configure / to set up", "configure")]           // leading "to" optional in each
    [InlineData("to configure / to set up", "to set up")]
    [InlineData("a lift / an elevator", "elevator")]                // article rules apply per wording
    public void Slash_separates_equivalent_wordings(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("configuration / config", "conf")]
    [InlineData("configuration / config", "configuration config")]  // not a single answer
    [InlineData("to configure / to set up", "set")]
    public void Slash_does_not_accept_anything_else(string expected, string typed)
    {
        Assert.False(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("look at sb/sth", "look at sb")]
    [InlineData("look at sb/sth", "look at something")]
    [InlineData("look [at/on] sb", "look at sb")]
    [InlineData("look [at/on] sb", "look on sb")]
    [InlineData("look [at / on] sb", "look on sb")]                 // spaced slash inside a group
    [InlineData("look [at/on] sb", "look sb")]                      // the group is still optional
    public void Slash_between_two_words_is_a_choice_for_that_position_only(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void Alternatives_combine_with_the_other_rules()
    {
        const string expected = "to take [good] care of sb / to look after sb";

        Assert.True(AnswerMatcher.Matches(expected, "take good care of somebody"));
        Assert.True(AnswerMatcher.Matches(expected, "look after sb"));
        Assert.True(AnswerMatcher.Matches(expected, "to look after someone"));
        Assert.False(AnswerMatcher.Matches(expected, "look after good sb"));
    }

    // ── Everything at once ────────────────────────────────────────────────────

    [Theory]
    [InlineData("to take [good] care of sb", "to take good care of somebody")]
    [InlineData("to take [good] care of sb", "take care of sb")]
    [InlineData("to take [good] care of sb", "take good care of someone")]
    public void Rules_combine(string expected, string typed)
    {
        Assert.True(AnswerMatcher.Matches(expected, typed));
    }

    [Theory]
    [InlineData("to take [good] care of sb", "take care sb")]      // dropped a required word
    [InlineData("to take [good] care of sb", "take great care of sb")]
    public void Rules_do_not_make_wrong_answers_pass(string expected, string typed)
    {
        Assert.False(AnswerMatcher.Matches(expected, typed));
    }

    // ── Display form ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("to look [at] sb", "to look at sb")]
    [InlineData("{to} and fro", "to and fro")]
    [InlineData("  take  [good]  care ", "take good care")]
    [InlineData(null, "")]
    public void Display_form_drops_markers_and_tidies_spacing(string? expected, string display)
    {
        Assert.Equal(display, AnswerMatcher.ToDisplayForm(expected));
    }

    [Theory]
    [InlineData("{to} and fro", "to and fro")]
    [InlineData("look [at] sb", "look [at] sb")]        // brackets stay: they inform the learner
    [InlineData("{a} few [more]", "a few [more]")]
    [InlineData(null, "")]
    public void Stripping_literal_markers_keeps_the_optional_ones(string? expected, string display)
    {
        Assert.Equal(display, AnswerMatcher.StripLiteralMarkers(expected));
    }

    // ── Degenerate input ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, "cat")]
    [InlineData("", "cat")]
    [InlineData("   ", "cat")]
    public void Empty_expectation_rejects_a_typed_answer(string? expected, string typed)
    {
        Assert.False(AnswerMatcher.Matches(expected, typed));
    }

    [Fact]
    public void Unclosed_marker_is_treated_as_plain_text_instead_of_swallowing_the_rest()
    {
        Assert.True(AnswerMatcher.Matches("look [at sb", "look at sb"));
    }
}
