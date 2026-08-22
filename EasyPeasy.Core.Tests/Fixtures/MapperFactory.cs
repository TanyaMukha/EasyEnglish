using AutoMapper;
using EasyPeasy.Core.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyPeasy.Core.Tests.Fixtures;

/// <summary>
/// Builds a single, shared <see cref="IMapper"/> from the real <see cref="MappingProfile"/> —
/// AutoMapper's built <see cref="IMapper"/> is stateless/thread-safe, so one instance is reused
/// across every test in the assembly instead of rebuilding the configuration per test class.
/// </summary>
public static class MapperFactory
{
    public static readonly IMapper Instance = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>(),
        NullLoggerFactory.Instance).CreateMapper();
}
