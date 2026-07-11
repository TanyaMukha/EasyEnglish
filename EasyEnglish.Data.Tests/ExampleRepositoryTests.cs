using EasyEnglish.Data.Repositories;
using EasyEnglish.Data.Tests.Fixtures;

namespace EasyEnglish.Data.Tests;

public class ExampleRepositoryTests : SqliteTestBase
{
    private ExampleRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task GetByUnitAsync_JoinsThroughWord_ReturnsOnlyThatUnitsExamples()
    {
        int unitAId;
        await using (var ctx = CreateContext())
        {
            var unitA = await TestDataHelpers.SeedUnitAsync(ctx, title: "Unit A");
            var unitB = await TestDataHelpers.SeedUnitAsync(ctx, title: "Unit B");
            unitAId = unitA.Id;

            var wordA = await TestDataHelpers.SeedWordAsync(ctx, unitA.Id, "wordA");
            var wordB = await TestDataHelpers.SeedWordAsync(ctx, unitB.Id, "wordB");

            await TestDataHelpers.SeedExampleAsync(ctx, wordA.Id, "Example for A1");
            await TestDataHelpers.SeedExampleAsync(ctx, wordA.Id, "Example for A2");
            await TestDataHelpers.SeedExampleAsync(ctx, wordB.Id, "Example for B");
        }

        var examples = await CreateRepository().GetByUnitAsync(unitAId);

        Assert.Equal(2, examples.Count);
        Assert.All(examples, e => Assert.StartsWith("Example for A", e.Sentence));
    }

    [Fact]
    public async Task GetByUnitAsync_NoExamples_ReturnsEmpty()
    {
        int unitId;
        await using (var ctx = CreateContext())
        {
            unitId = (await TestDataHelpers.SeedUnitAsync(ctx)).Id;
        }

        var examples = await CreateRepository().GetByUnitAsync(unitId);

        Assert.Empty(examples);
    }
}
