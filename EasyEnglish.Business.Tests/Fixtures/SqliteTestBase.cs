using AutoMapper;
using EasyEnglish.Business.Services;
using EasyEnglish.Core.Mapping;
using EasyEnglish.Data;
using EasyEnglish.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MukhaLab.Database;

namespace EasyEnglish.Business.Tests.Fixtures;

/// <summary>
/// Shared base for EasyEnglish.Business tests: opens one SQLite in-memory connection per test class
/// instance, builds the real <see cref="EasyEnglishDbContext"/> schema against it, and wires up all
/// 8 real services against real repositories (no mocking) — services here are thin enough that
/// mocking the repository layer would mostly just test the mock.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly IDbContextFactory<EasyEnglishDbContext> Factory;
    protected readonly IMapper Mapper;
    protected readonly IUserContext UserContext = new AnonymousUserContext();

    protected readonly SubjectService SubjectService;
    protected readonly ExampleService ExampleService;
    protected readonly IrregularFormService IrregularFormService;
    protected readonly StudyCardService StudyCardService;
    protected readonly TestCardService TestCardService;
    protected readonly WordService WordService;
    protected readonly UnitService UnitService;
    protected readonly CourseService CourseService;

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EasyEnglishDbContext>().UseSqlite(_connection).Options;
        Factory = new SimpleDbContextFactory(options);

        using (var ctx = Factory.CreateDbContext())
            ctx.Database.EnsureCreated();

        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

        SubjectService = new SubjectService(new SubjectRepository(Factory, UserContext), Mapper, NullLogger<SubjectService>.Instance);
        ExampleService = new ExampleService(new ExampleRepository(Factory, UserContext), Mapper, NullLogger<ExampleService>.Instance);
        IrregularFormService = new IrregularFormService(new IrregularFormRepository(Factory, UserContext), Mapper, NullLogger<IrregularFormService>.Instance);
        StudyCardService = new StudyCardService(new StudyCardRepository(Factory, UserContext), Mapper, NullLogger<StudyCardService>.Instance);
        TestCardService = new TestCardService(new TestCardRepository(Factory, UserContext), Mapper, NullLogger<TestCardService>.Instance);
        WordService = new WordService(new WordRepository(Factory, UserContext), Mapper, NullLogger<WordService>.Instance);

        UnitService = new UnitService(
            new UnitRepository(Factory, UserContext),
            Mapper,
            NullLogger<UnitService>.Instance,
            WordService,
            ExampleService,
            IrregularFormService,
            StudyCardService,
            TestCardService);

        CourseService = new CourseService(
            new CourseRepository(Factory, UserContext),
            WordService,
            Mapper,
            NullLogger<CourseService>.Instance,
            UnitService);
    }

    /// <summary>Opens a fresh <see cref="EasyEnglishDbContext"/> against the shared in-memory database — for arranging test data.</summary>
    protected EasyEnglishDbContext CreateContext() => Factory.CreateDbContext();

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class SimpleDbContextFactory(DbContextOptions<EasyEnglishDbContext> options) : IDbContextFactory<EasyEnglishDbContext>
    {
        public EasyEnglishDbContext CreateDbContext() => new(options);
    }
}
