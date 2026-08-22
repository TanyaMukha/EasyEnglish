using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EasyPeasy.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every <c>EasyPeasy.Core.Interfaces.Repositories.*</c> repository interface with its
    /// <c>EasyPeasy.Data.Repositories.*</c> implementation, scoped per DI scope. Does not register
    /// <c>IDbContextFactory&lt;EasyPeasyDbContext&gt;</c> or <c>MukhaLab.Database.IUserContext</c> —
    /// those are wired up separately (see <c>EasyPeasy.App</c>'s startup code).
    /// </summary>
    public static IServiceCollection AddEasyPeasyRepositories(this IServiceCollection services)
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