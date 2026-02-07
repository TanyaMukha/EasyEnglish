namespace MukhaLab.SelectQueryParameters.Models;

public class QueryParameters
{
    public int? PageNumber { get; init; }
    public int? RowCount { get; init; }
    public List<SortDescriptor>? Sort { get; init; }
    public List<FilterParameter>? Filters { get; init; }
}

/*
var queryParams = new QueryParameters
{
    PageNumber = 1,
    RowCount = 10,
    Filters = new List<FilterParameter>
    {
        new FilterParameter
        {
            Field = "Executors[Title]",
            Operation = FilterOperation.Contains,
            DataType = FilterDataType.String,
            Value = "John"
        }
    },
    Sort = new List<SortDescriptor>
    {
        new SortDescriptor { Field = "Title", Direction = SortDirection.Asc }
    }
};
*/
