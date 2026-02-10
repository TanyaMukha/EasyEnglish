using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Mapping;

/// <summary>
/// AutoMapper profile configuration for mapping between entities and models.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class
    /// and configures entity-model mappings.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<CourseEntity, CourseModel>().ReverseMap();

        CreateMap<WordEntity, WordModel>()
            .ForMember(dest => dest.CourseId,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.CourseId : 0));
        CreateMap<WordModel, WordEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        CreateMap<UpdateWordRateRequest, WordModel>().ReverseMap();
        CreateMap<UpdateWordRateRequest, WordEntity>();

        CreateMap<ExampleEntity, ExampleModel>().ReverseMap();
        CreateMap<UnitEntity, UnitModel>().ReverseMap();
        CreateMap<IrregularFormEntity, IrregularFormModel>().ReverseMap();
    }
}
