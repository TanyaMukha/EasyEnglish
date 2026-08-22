using EasyPeasy.Core.Mapping;
using EasyPeasy.Core.Tests.Fixtures;

namespace EasyPeasy.Core.Tests;

public class MappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new AutoMapper.MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Instance_IsUsable()
    {
        // Sanity check that the shared instance used by every other test class actually works.
        var model = MapperFactory.Instance.Map<EasyPeasy.Core.Models.SubjectModel>(
            new EasyPeasy.Core.Entities.SubjectEntity { Title = "English" });

        Assert.Equal("English", model.Title);
    }
}
