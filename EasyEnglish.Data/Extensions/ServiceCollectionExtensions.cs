using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every <c>EasyEnglish.Core.Interfaces.Repositories.*</c> repository interface with its
    /// <c>EasyEnglish.Data.Repositories.*</c> implementation, scoped per DI scope. Does not register
    /// <c>IDbContextFactory&lt;EasyEnglishDbContext&gt;</c> or <c>MukhaLab.Database.IUserContext</c> —
    /// those are wired up separately (see <c>EasyEnglish.App</c>'s startup code).
    /// </summary>
    public static IServiceCollection AddEasyEnglishRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IExampleRepository, ExampleRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IIrregularFormRepository, IrregularFormRepository>();
        services.AddScoped<IStudyCardRepository, StudyCardRepository>();
        services.AddScoped<ITestCardRepository, TestCardRepository>();

        return services;
    }
}