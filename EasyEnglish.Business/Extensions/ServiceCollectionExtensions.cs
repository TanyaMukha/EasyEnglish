using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Services.Extensions;

public static class ServiceCollectionExtensions
{
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