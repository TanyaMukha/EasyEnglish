using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEasyEnglishRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IExampleRepository, ExampleRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IIrregularFormRepository, IrregularFormRepository>();

        return services;
    }
}