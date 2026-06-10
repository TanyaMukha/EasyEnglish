using AutoMapper;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;

namespace EasyEnglish.App.Models;

/// <summary>
/// AutoMapper profile configuration for mapping between entities and models.
/// </summary>
public class MappingUIProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingUIProfile"/> class
    /// and configures entity-model mappings.
    /// </summary>
    public MappingUIProfile()
    {
        //CreateMap<UnitModel, UnitTestModel>()
        //    .ForMember(dest => dest.WordCount,
        //        opt => opt.MapFrom(src => src.Words != null ? src.Words.Count : 0))
        //    .ForMember(dest => dest.CourseName,
        //        opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : null));

        CreateMap<WordModel, WordTestModel>()
            .ForMember(dest => dest.CourseId,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.CourseId : 0))
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples))
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            // Явно скидаємо AfterMap який міг успадкуватись з WordModel→WordModel
            .AfterMap((src, dest, ctx) => { });

        CreateMap<WordTestModel, UpdateWordRateRequest>();
    }
}
