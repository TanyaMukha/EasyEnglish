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
    public MappingProfile()
    {
        // ========== SUBJECT ==========
        CreateMap<SubjectEntity, SubjectModel>();
        CreateMap<SubjectModel, SubjectEntity>();

        // ========== COURSE ==========
        CreateMap<CourseEntity, CourseModel>()
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Units))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject));

        CreateMap<CourseModel, CourseEntity>()
            .ForMember(dest => dest.Units, opt => opt.Ignore())
            .ForMember(dest => dest.Subject, opt => opt.Ignore());

        // ========== UNIT ==========
        CreateMap<UnitEntity, UnitModel>()
            .ForMember(dest => dest.Words, opt => opt.MapFrom(src => src.Words))
            .ForMember(dest => dest.IrregularForms, opt => opt.MapFrom(src => src.IrregularForms))
            .ForMember(dest => dest.StudyCards, opt => opt.MapFrom(src => src.StudyCards))
            .ForMember(dest => dest.TestCards, opt => opt.MapFrom(src => src.TestCards))
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course));

        CreateMap<UnitModel, UnitEntity>()
            .ForMember(dest => dest.Words, opt => opt.MapFrom(src => src.Words))
            .ForMember(dest => dest.IrregularForms, opt => opt.MapFrom(src => src.IrregularForms))
            .ForMember(dest => dest.StudyCards, opt => opt.MapFrom(src => src.StudyCards))
            .ForMember(dest => dest.TestCards, opt => opt.MapFrom(src => src.TestCards))
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // Model → Model з опціями
        CreateMap<UnitModel, UnitModel>()
            .ForMember(dest => dest.Words, opt => opt.MapFrom(src => src.Words))
            .ForMember(dest => dest.IrregularForms, opt => opt.MapFrom(src => src.IrregularForms))
            .ForMember(dest => dest.StudyCards, opt => opt.MapFrom(src => src.StudyCards))
            .ForMember(dest => dest.TestCards, opt => opt.MapFrom(src => src.TestCards))
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .AfterMap<UnitMappingAction>();

        // ========== WORD ==========
        CreateMap<WordEntity, WordModel>()
            .ForMember(dest => dest.CourseId,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.CourseId : 0))
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

        CreateMap<WordModel, WordEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples));

        // Model → Model з опціями
        CreateMap<WordModel, WordModel>()
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples))
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<WordMappingAction>();

        CreateMap<UpdateWordRateRequest, WordModel>().ReverseMap();
        
        CreateMap<UpdateWordRateRequest, WordEntity>();

        // ========== EXAMPLE ==========
        CreateMap<ExampleEntity, ExampleModel>();
        
        CreateMap<ExampleModel, ExampleEntity>()
            .ForMember(dest => dest.Word, opt => opt.Ignore());
        
        CreateMap<ExampleModel, ExampleModel>()
            .ForMember(dest => dest.Word, opt => opt.Ignore())
            .AfterMap<ExampleMappingAction>();

        // ========== IRREGULAR FORM ==========
        CreateMap<IrregularFormEntity, IrregularFormModel>();
        
        CreateMap<IrregularFormModel, IrregularFormEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        // Model → Model з опціями
        CreateMap<IrregularFormModel, IrregularFormModel>()
            .AfterMap<IrregularFormMappingAction>();

        CreateMap<UpdateWordRateRequest, IrregularFormEntity>();

        // ========== STUDY CARD ==========
        CreateMap<StudyCardEntity, StudyCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

        CreateMap<StudyCardModel, StudyCardEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        // Model → Model (потрібен для копіювання Unit при бекапі/експорті/імпорті —
        // UnitModel→UnitModel мапить свою колекцію StudyCards саме через цей self-map)
        CreateMap<StudyCardModel, StudyCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<StudyCardMappingAction>();

        // ========== TEST CARD ==========
        // Options/CorrectAnswers — гнучкі JSON-поля, структура залежить від Kind,
        // тож пакування/розпакування виконує кастомний ITypeConverter, а не ForMember.
        CreateMap<TestCardEntity, TestCardModel>()
            .ConvertUsing<TestCardEntityToModelConverter>();

        CreateMap<TestCardModel, TestCardEntity>()
            .ConvertUsing<TestCardModelToEntityConverter>();

        // Model → Model (той самий випадок, що й для StudyCard вище)
        CreateMap<TestCardModel, TestCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<TestCardMappingAction>();

        CreateMap<ChoicePayload, ChoicePayload>();
        CreateMap<ShortAnswerPayload, ShortAnswerPayload>();
        CreateMap<ClozePayload, ClozePayload>();
        CreateMap<MatchingPayload, MatchingPayload>();

        CreateMap<UpdateWordRateRequest, StudyCardEntity>();
        CreateMap<UpdateWordRateRequest, TestCardEntity>();
    }
} 