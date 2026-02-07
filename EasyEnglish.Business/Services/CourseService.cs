using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;
using EasyEnglish.Core.Options;
using EasyEnglish.Core.Enums;

namespace EasyEnglish.Services.Services;

public class CourseService : BaseService<CourseEntity, CourseModel>, ICourseService
{
    private readonly IUnitService unitService;
    private readonly IWordService wordService;

    public CourseService(
        ICourseRepository course,
        IWordService wordService,
        IMapper mapper,
        ILogger<CourseService> logger,
        IUnitService unitService)
        : base(course, mapper, logger)
    {
        this.unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
    }

    public async Task<IEnumerable<UnitModel>> GetUnitsAsync(int courseId)
    {
        QueryParameters parameters = new QueryParameters
        {
            Filters = new List<FilterParameter>
            {
                new FilterParameter
                {
                    Field = "CourseId",
                    Operation = FilterOperation.Equal,
                    DataType = FilterDataType.Integer,
                    Value = courseId
                }
            }
        };

        if (unitService is not null)
        {
            return await this.unitService.GetAllAsync(parameters, true);
        }
        else
        {
            return new List<UnitModel>();
        }
    }

    public async Task<IEnumerable<WordModel>> GetWordsAsync(int courseId, int? unitId = null, WordSelectionOptions? options = null)
    {
        List<FilterParameter> filters = new List<FilterParameter>
        {
            new FilterParameter
            {
                Field = "Unit.CourseId",
                Operation = FilterOperation.Equal,
                DataType = FilterDataType.Integer,
                Value = courseId
            },
        };

        if (unitId is not null)
        {
            filters.Add(
                new FilterParameter
                {
                    Field = "UnitId",
                    Operation = FilterOperation.Equal,
                    DataType = FilterDataType.Integer,
                    Value = unitId
                }
            );
        }

        if (options is not null && options.IncludeLearnedWords)
        {
            filters.Add(
                new FilterParameter
                {
                    Field = "Rate",
                    Operation = FilterOperation.GreaterThanOrEqual,
                    DataType = FilterDataType.Decimal,
                    Value = 1.6
                }
            );
        }

        List<SortDescriptor> sort = new();

        switch (options!.Priority)
        {
            case WordPriority.New:
                sort.Add(new SortDescriptor
                {
                    Field = "CreatedAt",
                    Direction = SortDirection.Desc,
                });
                filters.Add(new FilterParameter
                {
                    Field = "LastReviewDate",
                    Operation = FilterOperation.IsNull
                });
                break;
            case WordPriority.Random:
                sort.Add(new SortDescriptor
                {
                    Field = "CreatedAt",
                    Direction = SortDirection.Asc,
                });
                sort.Add(new SortDescriptor
                {
                    Field = "LastReviewDate",
                    Direction = SortDirection.Asc,
                });
                break;
            case WordPriority.Difficult:
                sort.Add(new SortDescriptor
                {
                    Field = "Rate",
                    Direction = SortDirection.Desc,
                });
                break;
            case WordPriority.Review:
                sort.Add(new SortDescriptor
                {
                    Field = "LastReviewDate",
                    Direction = SortDirection.Asc,
                });
                filters.Add(new FilterParameter
                {
                    Field = "LastReviewDate",
                    Operation = FilterOperation.IsNotNull
                });
                break;
            case WordPriority.Old:
                sort.Add(new SortDescriptor
                {
                    Field = "CreatedAt",
                    Direction = SortDirection.Asc,
                });
                filters.Add(new FilterParameter
                {
                    Field = "LastReviewDate",
                    Operation = FilterOperation.IsNull
                });
                break;
            default:
                break;
        }

        QueryParameters parameters = new QueryParameters
        {
            Filters = filters,
            RowCount = options is not null && options.WordCount > 0 ? options.WordCount : null,
            PageNumber = options is not null && options.WordCount > 0 ? 1 : null,
            Sort = sort
        };

        IEnumerable<WordModel> words = await this.wordService.GetAllAsync(parameters, true);

        if (options!.ShuffleWords)
        {
            words = words.OrderBy(x => Random.Shared.Next());
        }

        return words;
    }
}