using AutoMapper;
using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Mapping;

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

        // Model → Model with options (see UnitMappingOptions/UnitMappingAction)
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

        // Model → Model with options (see WordMappingOptions/WordMappingAction)
        CreateMap<WordModel, WordModel>()
            .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => src.Examples))
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<WordMappingAction>();

        // MemberList.Source: these are partial-update DTOs (Id/Rate/LastReviewDate/ReviewCount only),
        // so validation should check every *source* member is mapped, not every destination member.
        CreateMap<UpdateWordRateRequest, WordModel>(MemberList.Source).ReverseMap();

        CreateMap<UpdateWordRateRequest, WordEntity>(MemberList.Source);

        // ========== EXAMPLE ==========
        CreateMap<ExampleEntity, ExampleModel>();
        
        CreateMap<ExampleModel, ExampleEntity>()
            .ForMember(dest => dest.Word, opt => opt.Ignore());
        
        CreateMap<ExampleModel, ExampleModel>()
            .ForMember(dest => dest.Word, opt => opt.Ignore())
            .AfterMap<ExampleMappingAction>();

        // ========== IRREGULAR FORM ==========
        CreateMap<IrregularFormEntity, IrregularFormModel>()
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

        CreateMap<IrregularFormModel, IrregularFormEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        // Model → Model with options (see IrregularFormMappingOptions/IrregularFormMappingAction)
        CreateMap<IrregularFormModel, IrregularFormModel>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<IrregularFormMappingAction>();

        CreateMap<UpdateWordRateRequest, IrregularFormEntity>(MemberList.Source);

        // ========== STUDY CARD ==========
        CreateMap<StudyCardEntity, StudyCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

        CreateMap<StudyCardModel, StudyCardEntity>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        // Model → Model (needed for copying a Unit during backup/export/import —
        // UnitModel→UnitModel maps its StudyCards collection through exactly this self-map)
        CreateMap<StudyCardModel, StudyCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<StudyCardMappingAction>();

        // ========== TEST CARD ==========
        // Options/CorrectAnswers are flexible JSON fields whose structure depends on Kind,
        // so packing/unpacking goes through a custom ITypeConverter instead of ForMember.
        CreateMap<TestCardEntity, TestCardModel>()
            .ConvertUsing<TestCardEntityToModelConverter>();

        CreateMap<TestCardModel, TestCardEntity>()
            .ConvertUsing<TestCardModelToEntityConverter>();

        // Model → Model (same case as StudyCard above)
        CreateMap<TestCardModel, TestCardModel>()
            .ForMember(dest => dest.Unit, opt => opt.Ignore())
            .AfterMap<TestCardMappingAction>();

        CreateMap<ChoicePayload, ChoicePayload>();
        CreateMap<ShortAnswerPayload, ShortAnswerPayload>();
        CreateMap<ClozePayload, ClozePayload>();
        CreateMap<MatchingPayload, MatchingPayload>();

        CreateMap<UpdateWordRateRequest, StudyCardEntity>(MemberList.Source);
        CreateMap<UpdateWordRateRequest, TestCardEntity>(MemberList.Source);
    }
} 