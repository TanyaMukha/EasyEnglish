using EasyEnglish.Business.Tests.Fixtures;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Presets;

namespace EasyEnglish.Business.Tests;

/// <summary>
/// Tests for <see cref="EasyEnglish.Services.Services.UnitService.ReconcileAndUpdateAsync"/> — the
/// highest-risk logic in EasyEnglish.Business (GUID-based reconciliation across 5 child collections).
/// See EasyEnglish.Business/README.md Known Issues #4 for the null-collection mass-delete risk this
/// class specifically pins down.
/// </summary>
public class ReconcileAndUpdateAsyncTests : SqliteTestBase
{
    private async Task<(int UnitId, WordEntity Word1, WordEntity Word2, ExampleEntity Example1)> SeedUnitWithWordsAsync()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var word1 = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word1");
        var word2 = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word2");
        var example1 = await TestDataHelpers.SeedExampleAsync(ctx, word1.Id, "Example 1");
        return (unit.Id, word1, word2, example1);
    }

    [Fact]
    public async Task ExistingWord_MatchedByGuid_IsUpdatedInPlace_NotDuplicated()
    {
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Single(w => w.Id == word1.Id).Word = "updatedWord1";

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Id == word1.Id && w.Word == "updatedWord1");
    }

    [Fact]
    public async Task NewWord_WithFreshGuid_IsInsertedAsNewRow()
    {
        var (unitId, _, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words!.Add(new WordModel { Word = "brandNewWord", UnitId = unitId });

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(3, wordsInDb.Count);
        Assert.Contains(wordsInDb, w => w.Word == "brandNewWord");
    }

    [Fact]
    public async Task DeleteMissingTrue_RemovesWordNotInIncoming()
    {
        var (unitId, word1, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = incoming.Words!.Where(w => w.Id == word1.Id).ToList();

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: true);

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

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false);

        await using var ctx = CreateContext();
        var wordsInDb = ctx.Words.Where(w => w.UnitId == unitId).ToList();
        Assert.Equal(2, wordsInDb.Count);
    }

    [Fact]
    public async Task NullIncomingWords_WithDeleteMissingTrue_DeletesAllExistingWords()
    {
        // Documents the data-loss risk flagged in EasyEnglish.Business/README.md Known Issues #4:
        // a null child collection is indistinguishable from "explicitly emptied out."
        var (unitId, _, _, _) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        incoming!.Words = null;

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: true);

        await using var ctx = CreateContext();
        Assert.Empty(ctx.Words.Where(w => w.UnitId == unitId));
    }

    [Fact]
    public async Task ExampleReconciliation_NewExampleInserted_ExistingExampleUpdated()
    {
        var (unitId, word1, _, example1) = await SeedUnitWithWordsAsync();
        var incoming = await UnitService.GetByIdAsync(unitId, UnitIncludes.Full);
        var word1Model = incoming!.Words!.Single(w => w.Id == word1.Id);
        word1Model.Examples!.Single().Sentence = "Updated example";
        word1Model.Examples!.Add(new ExampleModel { Sentence = "New example", WordId = word1.Id });

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false);

        await using var ctx = CreateContext();
        var examples = ctx.Examples.Where(e => e.WordId == word1.Id).ToList();
        Assert.Equal(2, examples.Count);
        Assert.Contains(examples, e => e.Id == example1.Id && e.Sentence == "Updated example");
        Assert.Contains(examples, e => e.Sentence == "New example");
    }

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

        await UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false);

        await using var readCtx = CreateContext();
        var form = readCtx.IrregularForms.Single(f => f.Id == formId);
        Assert.Equal("run", form.FirstForm);
    }

    [Fact]
    public async Task UnitNotFound_ThrowsArgumentException()
    {
        var incoming = new UnitModel { Id = 999_999, Title = "Ghost" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            UnitService.ReconcileAndUpdateAsync(incoming, deleteMissing: false));
    }
}
