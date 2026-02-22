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
        // ========== COURSE ==========
        CreateMap<CourseEntity, CourseModel>()
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Units));

        CreateMap<CourseModel, CourseEntity>()
            .ForMember(dest => dest.Units, opt => opt.Ignore()); // ⚠️ Ignore зворотню навігацію

        // ========== UNIT ==========
        CreateMap<UnitEntity, UnitModel>()
            .ForMember(dest => dest.Words, opt => opt.MapFrom(src => src.Words))
            .ForMember(dest => dest.IrregularForms, opt => opt.MapFrom(src => src.IrregularForms))
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course));

        CreateMap<UnitModel, UnitEntity>()
            .ForMember(dest => dest.Words, opt => opt.Ignore())
            .ForMember(dest => dest.IrregularForms, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore()); // ⚠️ Ignore зворотню навігацію

        // ========== WORD ==========
        CreateMap<WordEntity, WordModel>()
            .ForMember(dest => dest.CourseId,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.CourseId : 0))
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit)); // Мапимо якщо завантажено

        CreateMap<WordModel, WordEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .ForMember(dest => dest.Examples, opt => opt.Ignore()); // ⚠️ Ignore зворотню навігацію

        CreateMap<UpdateWordRateRequest, WordModel>().ReverseMap();
        CreateMap<UpdateWordRateRequest, WordEntity>();

        // ========== EXAMPLE ==========
        CreateMap<ExampleEntity, ExampleModel>();

        CreateMap<ExampleModel, ExampleEntity>()
            .ForMember(dest => dest.Word, opt => opt.Ignore()); // ⚠️ Ignore зворотню навігацію

        // ========== IRREGULAR FORM ==========
        CreateMap<IrregularFormEntity, IrregularFormModel>();

        CreateMap<IrregularFormModel, IrregularFormEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore()); // ⚠️ Ignore зворотню навігацію

    }
}
