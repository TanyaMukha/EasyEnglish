using EasyPeasy.Business.Tests.Fixtures;
using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Options;
using EasyPeasy.Core.Presets;
using MukhaLab.Database;

namespace EasyPeasy.Business.Tests;

/// <summary>
/// Tests for <see cref="EasyPeasy.Business.Services.UnitService.ReconcileAndUpdateAsync"/> — the
/// highest-risk logic in EasyPeasy.Business (GUID-based reconciliation across 5 child collections,
/// plus partial-payload merging).
/// </summary>
public class ReconcileAndUpdateAsyncTests : SqliteTestBase
{
    /// <summary>Add/update only, progress merged newest-wins — the shape most tests here don't care about.</summary>
    private static readonly UnitMergeOptions AddOrUpdate = new() { DeleteMissing = false };

    /// <summary>Strict sync: children absent from the payload are deleted.</summary>
    private static readonly UnitMergeOptions StrictSync = new() { DeleteMissing = true };

    private async Task<(int UnitId, WordEntity Word1, WordEntity Word2, ExampleEntity Example1)> SeedUnitWithWordsAsync()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var word1 = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word1");
        var word2 = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word2");
        var example1 = await TestDataHelpers.SeedExampleAsync(ctx, word1.Id, "Example 1");
        return (unit.Id, word1, word2, example1);
    }

    // ── Identity: RecordGuid only, never Id ──────────────────────────────────

    [Fact]
    public async Task ExistingWord_MatchedByGuid_IsUpdatedInPlace_NotDuplicated()
    {
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Single(w => w.Id == word1.Id).Word = "updatedWord1";

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Id == word1.Id && w.Word == "updatedWord1");
    }

    [Fact]
    public async Task ForeignIdOnMatchedWord_IsOverwrittenByTheLocalRowsId()
    {
        // A payload from another app instance carries that instance's IDs. Matching is by GUID, so
        // the foreign Id must be discarded rather than followed to some unrelated local row.
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var target = incoming!.Words!.Single(w => w.Id == word1.Id);
        target.Id = 987_654;
        target.Word = "updatedViaGuid";

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Id == word1.Id && w.Word == "updatedViaGuid");
        Assert.DoesNotContain(wordsInDb, w => w.Id == 987_654);
    }

    [Fact]
    public async Task ForeignIdOnUnmatchedWord_IsZeroed_AndInsertedAsNewRow()
    {
        // Same idea for a word the local unit has never seen: keeping its foreign Id would make EF
        // insert with an explicit PK that may already belong to a different word.
        var (unitId, _, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Add(new WordModel { Word = "fromAnotherDevice", Id = 555_001, UnitId = unitId });

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(3, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Word == "fromAnotherDevice");
        Assert.DoesNotContain(wordsInDb, w => w.Id == 555_001);
    }

    [Fact]
    public async Task NewWord_WithFreshGuid_IsInsertedAsNewRow()
    {
        var (unitId, _, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Add(new WordModel { Word = "brandNewWord", UnitId = unitId });

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(3, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Word == "brandNewWord");
    }

    // ── DeleteMissing ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMissingTrue_RemovesWordNotInIncoming()
    {
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = incoming.Words!.Where(w => w.Id == word1.Id).ToList();

        await UnitService.ReconcileAndUpdateAsync(incoming, StrictSync);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Single(wordsInDb);
        Assert.Equal(word1.Id, wordsInDb[0].Id);
    }

    [Fact]
    public async Task DeleteMissingFalse_LeavesUnmatchedWordUntouched()
    {
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = incoming.Words!.Where(w => w.Id == word1.Id).ToList();

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
    }

    [Fact]
    public async Task NullIncomingWords_WithDeleteMissingTrue_LeavesExistingWordsUntouched()
    {
        // incoming.Words == null means "this payload doesn't say anything about words" -- it must
        // NOT be treated as "the words list is now empty," even with DeleteMissing: true. See
        // EasyPeasy.Business/README.md Known Issues #4.
        var (unitId, word1, word2, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = null;

        await UnitService.ReconcileAndUpdateAsync(incoming, StrictSync);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Id == word1.Id);
        Assert.Contains(wordsInDb, w => w.Id == word2.Id);
    }

    [Fact]
    public async Task ExplicitEmptyIncomingWords_WithDeleteMissingTrue_DeletesAllExistingWords()
    {
        // The escape hatch for callers who really do want to clear a unit's words: pass an explicit
        // empty list rather than null.
        var (unitId, _, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = [];

        await UnitService.ReconcileAndUpdateAsync(incoming, StrictSync);

        await using var ctx = CreateContext();
        Assert.Empty(ctx.Words.Where(w => w.UnitId == unitId));
    }

    // ── Examples ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task NullExamplesOnMatchedWord_LeavesExistingExamplesUntouched()
    {
        var (unitId, word1, _, example1) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Single(w => w.Id == word1.Id).Examples = null;

        await UnitService.ReconcileAndUpdateAsync(incoming, StrictSync);

        await using var ctx = CreateContext();
        var examples = ctx.Examples.Where(e => e.WordId == word1.Id).ToList();
        var example = Assert.Single(examples);
        Assert.Equal(example1.Id, example.Id);
    }

    [Fact]
    public async Task MergeExamplesFalse_EmptyIncomingExamples_LeavesExistingExamplesUntouched()
    {
        // A course archive exported *without* examples still deserializes each word with an empty
        // Examples list. Without MergeExamples that must read as "not included", not "deleted".
        var (unitId, word1, _, example1) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Single(w => w.Id == word1.Id).Examples = [];

        await UnitService.ReconcileAndUpdateAsync(
            incoming, new UnitMergeOptions { DeleteMissing = true, MergeExamples = false });

        await using var ctx = CreateContext();
        var example = Assert.Single(ctx.Examples.Where(e => e.WordId == word1.Id));
        Assert.Equal(example1.Id, example.Id);
        Assert.Equal("Example 1", example.Sentence);
    }

    [Fact]
    public async Task MergeExamplesTrue_EmptyIncomingExamples_WithDeleteMissing_DoesDeleteThem()
    {
        // The counterpart: when the payload *is* authoritative for examples, an explicit empty list
        // still means "the user removed them all."
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Single(w => w.Id == word1.Id).Examples = [];

        await UnitService.ReconcileAndUpdateAsync(
            incoming, new UnitMergeOptions { DeleteMissing = true, MergeExamples = true });

        await using var ctx = CreateContext();
        Assert.Empty(ctx.Examples.Where(e => e.WordId == word1.Id));
    }

    [Fact]
    public async Task ExampleReconciliation_NewExampleInserted_ExistingExampleUpdated()
    {
        var (unitId, word1, _, example1) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word1Model = incoming!.Words!.Single(w => w.Id == word1.Id);
        word1Model.Examples!.Single().Sentence = "Updated example";
        word1Model.Examples!.Add(new ExampleModel { Sentence = "New example", WordId = word1.Id });

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var ctx = CreateContext();
        var examples = ctx.Examples.Where(e => e.WordId == word1.Id).ToList();
        Assert.Equal(2, examples.Count);
        Assert.Contains(examples, e => e.Id == example1.Id && e.Sentence == "Updated example");
        Assert.Contains(examples, e => e.Sentence == "New example");
    }

    // ── Learning-progress merging ────────────────────────────────────────────

    private async Task<(int UnitId, int WordId)> SeedWordWithProgressAsync(
        float rate, DateTime? lastReviewDate, int reviewCount)
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var word = await TestDataHelpers.SeedWordAsync(
            ctx, unit.Id, "studied", rate, lastReviewDate, reviewCount);
        return (unit.Id, word.Id);
    }

    [Fact]
    public async Task KeepExisting_IncomingDefaults_DoNotWipeStoredProgress()
    {
        // The partial-archive case: exported without learning progress, so every word deserializes
        // with rate 3.0 / never-reviewed. Applying that verbatim would silently reset real progress.
        var reviewedOn = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var (unitId, wordId) = await SeedWordWithProgressAsync(1.5f, reviewedOn, 12);

        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word = incoming!.Words!.Single();
        word.Rate = 3.0f;
        word.LastReviewDate = null;
        word.ReviewCount = 0;

        await UnitService.ReconcileAndUpdateAsync(
            incoming,
            new UnitMergeOptions { LearningProgress = LearningProgressMerge.KeepExisting });

        await using var ctx = CreateContext();
        var stored = ctx.Words.Single(w => w.Id == wordId);
        Assert.Equal(1.5f, stored.Rate);
        Assert.Equal(reviewedOn, stored.LastReviewDate);
        Assert.Equal(12, stored.ReviewCount);
    }

    [Fact]
    public async Task PreferNewest_IncomingReviewedLater_Wins()
    {
        var (unitId, wordId) = await SeedWordWithProgressAsync(
            2.0f, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 3);

        var newer = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word = incoming!.Words!.Single();
        word.Rate = 4.5f;
        word.LastReviewDate = newer;
        word.ReviewCount = 9;

        await UnitService.ReconcileAndUpdateAsync(
            incoming,
            new UnitMergeOptions { LearningProgress = LearningProgressMerge.PreferNewest });

        await using var ctx = CreateContext();
        var stored = ctx.Words.Single(w => w.Id == wordId);
        Assert.Equal(4.5f, stored.Rate);
        Assert.Equal(newer, stored.LastReviewDate);
        Assert.Equal(9, stored.ReviewCount);
    }

    [Fact]
    public async Task PreferNewest_StoredReviewedLater_Wins()
    {
        // Importing an older archive over freshly-studied local data must not roll progress back.
        var storedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var (unitId, wordId) = await SeedWordWithProgressAsync(1.2f, storedDate, 20);

        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word = incoming!.Words!.Single();
        word.Rate = 4.0f;
        word.LastReviewDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        word.ReviewCount = 2;

        await UnitService.ReconcileAndUpdateAsync(
            incoming,
            new UnitMergeOptions { LearningProgress = LearningProgressMerge.PreferNewest });

        await using var ctx = CreateContext();
        var stored = ctx.Words.Single(w => w.Id == wordId);
        Assert.Equal(1.2f, stored.Rate);
        Assert.Equal(storedDate, stored.LastReviewDate);
        Assert.Equal(20, stored.ReviewCount);
    }

    [Fact]
    public async Task PreferNewest_NeverReviewedIncoming_DoesNotWipeStoredProgress()
    {
        // A never-reviewed item counts as oldest, so a brand-new copy of a word can't erase the
        // history the local database already has for it.
        var storedDate = new DateTime(2026, 4, 4, 0, 0, 0, DateTimeKind.Utc);
        var (unitId, wordId) = await SeedWordWithProgressAsync(0.9f, storedDate, 30);

        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word = incoming!.Words!.Single();
        word.Rate = 3.0f;
        word.LastReviewDate = null;
        word.ReviewCount = 0;

        await UnitService.ReconcileAndUpdateAsync(
            incoming,
            new UnitMergeOptions { LearningProgress = LearningProgressMerge.PreferNewest });

        await using var ctx = CreateContext();
        var stored = ctx.Words.Single(w => w.Id == wordId);
        Assert.Equal(0.9f, stored.Rate);
        Assert.Equal(storedDate, stored.LastReviewDate);
        Assert.Equal(30, stored.ReviewCount);
    }

    // ── Other child collections ──────────────────────────────────────────────

    [Fact]
    public async Task IrregularFormReconciliation_MatchesExistingRowByGuid()
    {
        int unitId, formId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            unitId = unit.Id;
            formId = (await TestDataHelpers.SeedIrregularFormAsync(ctx, unit.Id, "go")).Id;
        }

        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.IrregularForms!.Single().FirstForm = "run";

        await UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate);

        await using var readCtx = CreateContext();
        var form = readCtx.IrregularForms.Single(f => f.Id == formId);
        Assert.Equal("run", form.FirstForm);
    }

    [Fact]
    public async Task UnitNotFound_ThrowsEntityNotFoundException()
    {
        var incoming = new UnitModel { Id = 999_999, Title = "Ghost" };

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            UnitService.ReconcileAndUpdateAsync(incoming, AddOrUpdate));
    }
}
