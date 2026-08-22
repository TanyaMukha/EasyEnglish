using EasyPeasy.Business.Services;
using EasyPeasy.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyPeasy.Business.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every <c>EasyPeasy.Core.Interfaces.Services.*</c> service interface with its
    /// <c>EasyPeasy.Business.Services.*</c> implementation, scoped per DI scope. Requires
    /// <c>AddEasyPeasyRepositories()</c> (from <c>EasyPeasy.Data</c>) to also be called, plus an
    /// <c>IMapper</c> registration — neither is registered here.
    /// </summary>
    public static IServiceCollection AddEasyPeasyDataServices(this IServiceCollection services)
    {
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IWordService, WordService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IStudyCardService, StudyCardService>();
        services.AddScoped<ITestCardService, TestCardService>();
        services.AddScoped<IExampleService, ExampleService>();
        services.AddScoped<IIrregularFormService, IrregularFormService>();

        return services;
    }
}