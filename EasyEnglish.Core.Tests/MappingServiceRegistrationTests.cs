using AutoMapper;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Extensions;
using EasyEnglish.Core.Mapping;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyEnglish.Core.Tests;

/// <summary>
/// Guards the container side of the mapping setup. Under <c>AddAutoMapper</c> the mapper builds every
/// <see cref="IMappingAction{TSource,TDestination}"/>/<see cref="ITypeConverter{TSource,TDestination}"/>
/// through the container instead of <c>new</c>-ing it, so a type missing from
/// <see cref="ServiceCollectionExtensions.AddEasyEnglishMappingServices"/> doesn't fail at startup —
/// it fails the first time a map reaches the member that uses it (which is how the unit export broke
/// on <c>TestCards</c>, the only member whose action was unregistered *and* had data to map).
/// </summary>
public class MappingServiceRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance); // AddAutoMapper resolves one
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddEasyEnglishMappingServices();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void EveryMappingServiceInTheProfileAssembly_IsResolvable()
    {
        using var provider = BuildProvider();

        var mappingServices = typeof(MappingProfile).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true })
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IMappingAction<,>) ||
                 i.GetGenericTypeDefinition() == typeof(ITypeConverter<,>))))
            .ToList();

        Assert.NotEmpty(mappingServices);

        var unresolved = mappingServices.Where(t => provider.GetService(t) is null).Select(t => t.Name).ToList();
        Assert.True(unresolved.Count == 0, "Not registered: " + string.Join(", ", unresolved));
    }

    /// <summary>
    /// The export path: a unit is cloned through the <c>UnitModel → UnitModel</c> self-map with a mapper
    /// resolved from the container, so every child collection exercises its mapping action.
    /// </summary>
    [Fact]
    public void UnitSelfMap_ThroughContainerResolvedMapper_MapsEveryChildCollection()
    {
        using var provider = BuildProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var unit = new UnitModel
        {
            Id = 10,
            CourseId = 5,
            Words = [new WordModel { Id = 1, UnitId = 10, Word = "apple", Examples = [new ExampleModel { Id = 11, WordId = 1 }] }],
            IrregularForms = [new IrregularFormModel { Id = 2, UnitId = 10, FirstForm = "go" }],
            StudyCards = [new StudyCardModel { Id = 3, UnitId = 10 }],
            TestCards = [new TestCardModel { Id = 4, UnitId = 10, Kind = TestCardKind.SingleChoice }],
        };

        var options = new UnitMappingOptions
        {
            ResetId = true,
            Word = new() { ResetId = true },
            IrregularForm = new() { ResetId = true },
            StudyCard = new() { ResetId = true },
            TestCard = new() { ResetId = true },
        };

        var clone = mapper.Map<UnitModel>(unit, opts => opts.Items[UnitMappingOptions.Key] = options);

        Assert.Equal(0, clone.Id);
        Assert.Equal(0, clone.Words![0].Id);
        Assert.Equal(0, clone.IrregularForms![0].Id);
        Assert.Equal(0, clone.StudyCards![0].Id);
        Assert.Equal(0, clone.TestCards![0].Id);
        Assert.Equal(0, clone.TestCards[0].UnitId);
    }
}
