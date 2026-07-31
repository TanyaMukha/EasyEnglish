using AutoMapper;
using EasyEnglish.Core.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyEnglish.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every helper type <see cref="MappingProfile"/> hands to AutoMapper —
    /// <see cref="IMappingAction{TSource,TDestination}"/>, <see cref="ITypeConverter{TSource,TDestination}"/>,
    /// <see cref="IValueResolver{TSource,TDestination,TDestMember}"/> and
    /// <see cref="IMemberValueResolver{TSource,TDestination,TSourceMember,TDestMember}"/> implementations —
    /// as transient. <c>AddAutoMapper</c> makes AutoMapper construct these through the container, so an
    /// unregistered one fails at map time (not at startup) with "Cannot create an instance of type X",
    /// and only for the member that actually uses it. Scanning the assembly keeps that list from going
    /// stale as new actions/converters are added to the profile.
    /// </summary>
    public static IServiceCollection AddEasyEnglishMappingServices(this IServiceCollection services)
    {
        foreach (var type in typeof(MappingProfile).Assembly.GetTypes().Where(IsMappingService))
            services.TryAddTransient(type);

        return services;
    }

    private static bool IsMappingService(Type type) =>
        type is { IsAbstract: false, IsClass: true, IsGenericTypeDefinition: false } &&
        type.GetInterfaces().Any(i => i.IsGenericType && MappingServiceInterfaces.Contains(i.GetGenericTypeDefinition()));

    private static readonly HashSet<Type> MappingServiceInterfaces =
    [
        typeof(IMappingAction<,>),
        typeof(ITypeConverter<,>),
        typeof(IValueResolver<,,>),
        typeof(IMemberValueResolver<,,,>),
    ];
}
