# MukhaLab.SelectQueryParameters

A tiny, dependency-free .NET 9 library that turns a serializable **filter / sort / page**
description (`QueryParameters`) into `Expression` trees applied to any `IQueryable<T>` — most
commonly an EF Core `DbSet<T>`. It lets an API accept dynamic, client-driven query parameters
(grid filters, column sorting, pagination) without hand-writing `Where`/`OrderBy`/`Skip`/`Take`
for every field and every entity.

- **Target framework:** `net9.0`
- **Namespace root:** `MukhaLab.SelectQueryParameters`
- **External dependencies:** none — only `System.Linq` / `System.Linq.Expressions` from the BCL
- **Used by:** [`MukhaLab.Database`](../MukhaLab.Database) (`BaseRepository<T, TContext>`, `BaseService<TEntity, TModel>`)

## Table of contents

- [Project layout](#project-layout)
- [Installation](#installation)
- [Core types](#core-types)
- [Quick start](#quick-start)
- [Field path syntax](#field-path-syntax)
- [Filter operations](#filter-operations)
- [Data types & conversion](#data-types--conversion)
- [Sorting](#sorting)
- [Paging](#paging)
- [Putting it all together](#putting-it-all-together)
- [Integration with MukhaLab.Database](#integration-with-mukhalabdatabase)
- [Known limitations & gotchas](#known-limitations--gotchas)
- [Extending the library](#extending-the-library)
- [Troubleshooting](#troubleshooting)

## Project layout

```
MukhaLab.SelectQueryParameters/
├── Models/
│   ├── FilterDataType.cs      # enum: target CLR type for a filter value
│   ├── FilterOperation.cs     # enum: comparison to apply
│   ├── FilterValue.cs         # abstract base: Field + Operation + DataType
│   ├── FilterParameter.cs     # concrete filter: adds Value / From / To
│   ├── SortDirection.cs       # enum: Asc / Desc
│   ├── SortDescriptor.cs      # one sort key: Field + Direction
│   └── QueryParameters.cs     # aggregate: PageNumber, RowCount, Sort, Filters
└── Extensions/
    └── QueryHelperExtension.cs  # QueryHelperExtensions — the expression-tree engine
```

## Installation

The library is a plain project reference inside the EasyEnglish solution — there is no separate
NuGet package. Add a `ProjectReference` to `MukhaLab.SelectQueryParameters.csproj` and import the
two namespaces:

```csharp
using MukhaLab.SelectQueryParameters.Models;      // QueryParameters, FilterParameter, SortDescriptor, ...
using MukhaLab.SelectQueryParameters.Extensions;   // ApplyQueryParameters and friends
```

## Core types

| Type | Role |
|---|---|
| `QueryParameters` | Aggregate root: `PageNumber`, `RowCount`, `Sort` (`List<SortDescriptor>`), `Filters` (`List<FilterParameter>`). This is the object you receive from a client (deserialized from JSON/query string) and pass to `ApplyQueryParameters`. |
| `FilterValue` (abstract) | Base for filter descriptions: `Field` (property path), `Operation` (`FilterOperation`), `DataType` (`FilterDataType`), `IsValid()`. |
| `FilterParameter : FilterValue` | The concrete filter type actually used. Adds `Value` (single-value operations), `From`/`To` (`Between`). All three are untyped `object` because they typically arrive as strings from a request. |
| `FilterOperation` | `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `Contains`, `StartsWith`, `EndsWith`, `Between`, `IsNull`, `IsNotNull`. |
| `FilterDataType` | `String`, `Integer`, `Decimal`, `DateTime`, `Date`, `Boolean`, `Guid` — tells the engine how to convert the raw `object` value before building the expression. |
| `SortDescriptor` | One sort key: `Field` + `Direction` (`SortDirection.Asc`/`Desc`). |
| `QueryHelperExtensions` | Static class with the `IQueryable<T>` extension methods that do the actual work. |

## Quick start

```csharp
using MukhaLab.SelectQueryParameters.Models;
using MukhaLab.SelectQueryParameters.Extensions;

// WordEntity: Word (string), Translation (string), UnitId (int),
// Unit (navigation -> UnitEntity.Title), LastReviewDate (DateTime?), Examples (List<ExampleEntity>)

var parameters = new QueryParameters
{
    PageNumber = 1,
    RowCount = 20,
    Filters = new List<FilterParameter>
    {
        new FilterParameter
        {
            Field = "Word",
            Operation = FilterOperation.Contains,
            DataType = FilterDataType.String,
            Value = "app"
        }
    },
    Sort = new List<SortDescriptor>
    {
        new SortDescriptor { Field = "Word", Direction = SortDirection.Asc }
    }
};

IQueryable<WordEntity> query = dbContext.Words;
List<WordEntity> page = query.ApplyQueryParameters(parameters).ToList();
```

`ApplyQueryParameters` composes three independent steps, in this fixed order:

1. **Filters** (`ApplyFilters`) — every entry in `Filters` becomes its own `Where(...)` clause,
   AND-combined.
2. **Sorting** (`ApplySorting`) — the first `SortDescriptor` becomes `OrderBy`/`OrderByDescending`,
   the rest become `ThenBy`/`ThenByDescending`.
3. **Paging** (`ApplyPaging`) — `Skip`/`Take`, only when both `PageNumber` and `RowCount` are set.

Each step is a no-op when its corresponding data is absent, so a `QueryParameters` with only
`Filters` set will filter without sorting or paging.

You can also call the individual extension methods directly if you only need one behavior:

```csharp
query = query.ApplyFilters(parameters.Filters);
query = query.ApplySorting(parameters.Sort);
query = query.ApplyPaging(pageNumber: 2, pageSize: 20);
```

## Field path syntax

`FilterValue.Field` and `SortDescriptor.Field` accept three path forms:

| Syntax | Example | Meaning (filtering) |
|---|---|---|
| Simple property | `"Word"` | `x.Word` |
| Dot-separated navigation | `"Unit.Title"` | `x.Unit.Title` |
| Collection segment | `"Examples[Sentence]"` | `x.Examples.Any(i => <comparison against i.Sentence>)` |

A collection segment `"Collection[Property]"` filters by comparing the collection item's
`Property` against `FilterParameter.Value`/`From`/`To`, using the same `Operation`/`DataType` as a
normal filter — e.g. `Field = "Examples[Sentence]"`, `Operation = Contains`, `Value = "hello"`
becomes `x.Examples.Any(i => i.Sentence.Contains("hello"))`. The bracket's inner path can itself
be a dot-separated navigation (e.g. `"Examples[Author.Name]"`), and the collection segment can be
preceded by a dot-separated prefix (e.g. `"Unit.Executors[Title]"`).

Sorting (`SortDescriptor.Field`) resolves collection segments differently — see
[Sorting](#sorting) — because there is no single "value" to sort by for a collection; only
filtering compares against an actual value.

> **Dot-separated navigation never null-checks intermediate steps.** `"Unit.Executors[Title]"`
> resolves to `x.Unit.Executors...` with no null guard on `x.Unit` — under a real, SQL-translating
> provider (EF Core, etc.) this is safe: SQL's `NULL` semantics mean a missing intermediate row
> simply produces no match, not an error. Evaluated directly against LINQ-to-Objects
> (`IEnumerable<T>.AsQueryable()`), however, a `null` intermediate throws `NullReferenceException`,
> since there is no such automatic null-propagation in plain C#. This only matters for in-memory
> testing/usage; against EF Core it behaves as expected.

## Filter operations

| `FilterOperation` | Generated expression | Requirements |
|---|---|---|
| `Equal` | `x.Field == Value` | — |
| `NotEqual` | `x.Field != Value` | — |
| `GreaterThan` | `x.Field > Value` | `Field` type must support `>` (numbers, `DateTime`, ...). Not applicable to `string`. |
| `GreaterThanOrEqual` | `x.Field >= Value` | Same as above. |
| `LessThan` | `x.Field < Value` | Same as above. |
| `LessThanOrEqual` | `x.Field <= Value` | Same as above. |
| `Contains` | `x.Field.Contains(Value)` | `Field` must be `string`. |
| `StartsWith` | `x.Field.StartsWith(Value)` | `Field` must be `string`. |
| `EndsWith` | `x.Field.EndsWith(Value)` | `Field` must be `string`. |
| `Between` | `x.Field >= From && x.Field <= To` | Uses `From`/`To`, not `Value`. Inclusive on both ends. |
| `IsNull` | `x.Field == null` (or constant `false` for a non-nullable value type) | For reference types and `Nullable<T>` this is a normal null check. For a non-nullable value type (`int`, non-nullable `DateTime`, ...), which can never be `null`, this resolves to a constant `false` instead of throwing. |
| `IsNotNull` | `x.Field != null` (or constant `true` for a non-nullable value type) | Mirror of `IsNull` — resolves to constant `true` for a non-nullable value type. |

`FilterParameter.ToString()` returns `Field`, which is convenient for logging/diagnostics
("which filter caused this query to fail").

## Data types & conversion

`FilterDataType` tells `ConvertFilterValue` how to turn the boxed `Value`/`From`/`To` into the CLR
type the comparison needs:

```csharp
new FilterParameter
{
    Field = "LastReviewDate",
    Operation = FilterOperation.GreaterThanOrEqual,
    DataType = FilterDataType.Date,
    Value = "2026-01-01"
}
```

Conversion rules:

- The value is first converted to a string with `.ToString()` (so a boxed `int` and its string
  form are handled the same way), then parsed with the matching `Convert.ToXxx`/`Guid.Parse` call.
- `null` and blank/whitespace-only strings both convert to `null`.
- `FilterDataType.Date` truncates the parsed `DateTime` to its date component (`.Date`).
- Numeric, date, and boolean conversions use `CultureInfo.InvariantCulture` explicitly (e.g. `"."`
  as the decimal separator) rather than the current thread/OS culture — filter values normally
  arrive from a query string or JSON payload in a culture-neutral format, so parsing them with the
  ambient culture would make a value like `"1.50"` fail on a machine whose culture uses `","` as
  the decimal separator.
- Conversion failures (e.g. `DataType.Integer` with `Value = "abc"`) throw a `FormatException`
  from the underlying `Convert.ToXxx` call — validate input before constructing `FilterParameter`
  instances from untrusted request data.

## Sorting

```csharp
Sort = new List<SortDescriptor>
{
    new SortDescriptor { Field = "Unit.Title", Direction = SortDirection.Asc },
    new SortDescriptor { Field = "Word",       Direction = SortDirection.Asc }
}
```

`ApplySorting` walks the list in order: the **first** non-blank descriptor becomes
`OrderBy`/`OrderByDescending`, every descriptor after that becomes `ThenBy`/`ThenByDescending`.
Descriptors with a blank `Field` are skipped, so you can safely include placeholder rows from a
UI form.

Sorting by a collection path (e.g. `"Examples[Sentence]"`) is technically possible but resolves to
`x.Examples.Any(i => i.Sentence != null)` — an existence check for a non-null nested property, not
a value to sort by. This differs from how the same path is used for **filtering** (which compares
by actual value, see [Field path syntax](#field-path-syntax)); sorting has no single "value" to
extract from a collection, so it falls back to this existence check. Avoid sorting by collection
paths unless that existence semantics is genuinely what you want.

## Paging

```csharp
query.ApplyPaging(pageNumber: 2, pageSize: 20); // rows 21..40
```

`ApplyPaging` clamps out-of-range input instead of throwing: `pageNumber < 1` becomes `1`, and
`pageSize < 1` becomes `10`. It performs plain `Skip((pageNumber - 1) * pageSize).Take(pageSize)`,
so the query should already be sorted for the result to be stable across pages.

`ApplyQueryParameters` only pages when **both** `QueryParameters.PageNumber` and
`QueryParameters.RowCount` have a value.

The `QueryParameters` constructor only defaults `PageNumber` to `1` when `rowCount` is supplied and
`pageNumber` is not — i.e. when paging was actually requested but no explicit page was given. An
explicitly supplied `pageNumber` is always used as given, and `PageNumber` stays `null` when
neither value is provided:

```csharp
new QueryParameters().PageNumber                              // == null, RowCount == null -> paging not applied
new QueryParameters(rowCount: 20).PageNumber                   // == 1 (defaulted: paging requested, no page given)
new QueryParameters(pageNumber: 3, rowCount: 20).PageNumber    // == 3 (explicit value used as given)
new QueryParameters(pageNumber: 5).PageNumber                  // == 5 (explicit value preserved even without RowCount)
```

## Putting it all together

```csharp
var parameters = new QueryParameters
{
    PageNumber = 1,
    RowCount = 25,
    Filters = new List<FilterParameter>
    {
        new FilterParameter
        {
            Field = "Unit.Title",
            Operation = FilterOperation.Equal,
            DataType = FilterDataType.String,
            Value = "Travel"
        },
        new FilterParameter
        {
            Field = "LastReviewDate",
            Operation = FilterOperation.Between,
            DataType = FilterDataType.Date,
            From = "2026-01-01",
            To = "2026-06-30"
        }
    },
    Sort = new List<SortDescriptor>
    {
        new SortDescriptor { Field = "Rate", Direction = SortDirection.Desc },
        new SortDescriptor { Field = "Word", Direction = SortDirection.Asc }
    }
};

var words = dbContext.Words
    .Include(w => w.Unit)
    .ApplyQueryParameters(parameters)
    .ToList();
```

Because everything is built with `System.Linq.Expressions`, EF Core translates the resulting query
into a single SQL statement — filtering, sorting, and paging all happen in the database, not in
memory.

## Integration with MukhaLab.Database

[`MukhaLab.Database`](../MukhaLab.Database) wires `QueryParameters` straight into its generic
repository/service base classes, so any repository derived from `BaseRepository<T, TContext>`
gets dynamic querying for free:

```csharp
// IBaseRepository<T>
Task<IEnumerable<T>> GetAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default);
Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
```

`BaseRepository<T, TContext>` (see [`BaseRepository.cs`](../MukhaLab.Database/BaseRepository.cs))
builds the base `IQueryable<T>` (including any configured user-scoping filter and `Include`
paths) and then calls `ApplyQueryParameters` on it:

```csharp
private IQueryable<T> BuildSelectQuery(TContext ctx, QueryParameters parameters, bool withoutPagination = false, string[]? includes = null)
{
    var queryParameters = withoutPagination
        ? new QueryParameters { Filters = parameters.Filters, Sort = parameters.Sort, PageNumber = null, RowCount = null }
        : parameters;

    return BuildSelectQuery(ctx, includes).ApplyQueryParameters(queryParameters);
}
```

`GetPaginationInfoAsync` reuses the same filters with paging stripped out to compute
`TotalCount`/`TotalPages` for the same filtered set. Consuming a repository:

```csharp
public class WordRepository : BaseRepository<WordEntity, EasyEnglishDbContext>, IWordRepository
{
    public WordRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext) { }
}

// Elsewhere:
var parameters = new QueryParameters(pageNumber: 1, rowCount: 20,
    filters: new() { new FilterParameter { Field = "Word", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "run" } });

var page = await wordRepository.GetAsync(parameters, includes: new[] { nameof(WordEntity.Unit) });
var pagination = await wordRepository.GetPaginationInfoAsync(parameters);
```

`BaseService<TEntity, TModel>` forwards `GetAllAsync(QueryParameters, ...)` and
`GetPaginationInfoAsync(QueryParameters, ...)` to the repository and maps entities to view models
with AutoMapper — see [`BaseService.cs`](../MukhaLab.Database/BaseService.cs).

## Known limitations & gotchas

- **`GreaterThan`/`LessThan`(`OrEqual`) don't work on `string`.** `Expression.GreaterThan` requires
  a native comparison operator; strings don't have one. Use `Contains`/`StartsWith`/`EndsWith` for
  string comparisons, or compare a parsed value (e.g. `DateTime`, numeric) instead.
- **`Contains`/`StartsWith`/`EndsWith` require a `string` property.** Applying them to a
  non-string field throws at expression-build time.
- **Conversion errors surface as `FormatException`/`OverflowException`.** `ConvertFilterValue` does
  not catch parsing errors from `Convert.ToXxx`/`Guid.Parse`; validate `DataType` vs. `Value` before
  building a `FilterParameter` from untrusted input (e.g. an HTTP request).
- **Values are `object`, so there is no compile-time type safety.** Mismatches between `DataType`
  and the entity property's actual type are only caught at query-build time (an exception), not at
  compile time.
- **Sorting by a collection path uses existence semantics, not value semantics** (unlike filtering
  by the same path). See [Sorting](#sorting).

## Extending the library

The engine has two natural extension points, both in
[`QueryHelperExtension.cs`](Extensions/QueryHelperExtension.cs):

- **New `FilterOperation`:** add the enum member to
  [`FilterOperation.cs`](Models/FilterOperation.cs), then add a matching arm to the `switch` inside
  `BuildComparisonExpression` — it is shared by plain-property filtering and collection-path
  filtering, so one change covers both.
- **New `FilterDataType`:** add the enum member to
  [`FilterDataType.cs`](Models/FilterDataType.cs), then add a matching arm to the `switch` inside
  `ConvertFilterValue`.

Both switches are exhaustive over their enum today, so the compiler will not warn you about a
missing case — remember to update both the model and the extension method together.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| `ArgumentException` about method `Contains`/`StartsWith`/`EndsWith` not applicable | The filter targets a non-`string` property. |
| `InvalidOperationException`/`ArgumentException` from `Expression.GreaterThan`/`LessThan` | Those operations were used on a `string` property, which has no native comparison operator. |
| `FormatException` / `OverflowException` from `Convert.ToXxx` | `DataType` doesn't match the shape of `Value`/`From`/`To` (e.g. `DataType.Integer` with a non-numeric string). |
| A collection sort (`"Collection[Property]"` in `SortDescriptor.Field`) doesn't order by the value you expect | Expected — sorting by a collection path uses existence semantics, not value semantics; see [Sorting](#sorting). |
| Paging silently doesn't happen | `QueryParameters.RowCount` is `null` — both `PageNumber` and `RowCount` must be set. |
