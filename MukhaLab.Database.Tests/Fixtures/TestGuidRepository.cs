using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

public class TestGuidRepository : BaseWithGuidRepository<TestGuidEntity, TestDbContext>
{
    public TestGuidRepository(IDbContextFactory<TestDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }
}
