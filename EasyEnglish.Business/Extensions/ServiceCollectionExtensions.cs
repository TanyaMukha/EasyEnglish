using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every <c>EasyEnglish.Core.Interfaces.Services.*</c> service interface with its
    /// <c>EasyEnglish.Services.Services.*</c> implementation, scoped per DI scope. Requires
    /// <c>AddEasyEnglishRepositories()</c> (from <c>EasyEnglish.Data</c>) to also be called, plus an
    /// <c>IMapper</c> registration — neither is registered here.
    /// </summary>
    public static IServiceCollection AddEasyEnglishDataServices(this IServiceCollection services)
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