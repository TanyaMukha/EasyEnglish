using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

public class TestMappingProfile : Profile
{
    public TestMappingProfile()
    {
        CreateMap<TestEntity, TestModel>().ReverseMap();
    }
}

public class TestService : BaseService<TestEntity, TestModel>
{
    public TestService(IBaseRepository<TestEntity> repository, IMapper mapper, ILogger<BaseService<TestEntity, TestModel>> logger)
        : base(repository, mapper, logger)
    {
    }
}
