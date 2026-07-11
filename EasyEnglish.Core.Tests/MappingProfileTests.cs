using EasyEnglish.Core.Mapping;
using EasyEnglish.Core.Tests.Fixtures;

namespace EasyEnglish.Core.Tests;

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
        var model = MapperFactory.Instance.Map<EasyEnglish.Core.Models.SubjectModel>(
            new EasyEnglish.Core.Entities.SubjectEntity { Title = "English" });

        Assert.Equal("English", model.Title);
    }
}
