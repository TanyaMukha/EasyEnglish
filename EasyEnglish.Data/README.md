# EasyEnglish.Data

The persistence layer: the EF Core `DbContext`, SQLite migrations, and the concrete repository
implementations behind `EasyEnglish.Core`'s `I*Repository` interfaces. Everything here is EF
Core/SQLite-specific — `EasyEnglish.Business` and `EasyEnglish.App` depend on the `I*Repository`
interfaces from `EasyEnglish.Core`, never on this project's concrete types directly (only the
DI registration extension, `AddEasyEnglishRepositories()`, is called from app startup).

## Project layout

| File / folder | Purpose |
|---|---|
| `EasyEnglishDbContext.cs` | The `DbContext`. No `OnModelCreating` override — schema comes entirely from data annotations on `EasyEnglish.Core.Entities.*`. Also auto-stamps `CreatedAt`/`UpdatedAt` on save. |
| `DesignTimeDbContextFactory.cs` | `IDesignTimeDbContextFactory<EasyEnglishDbContext>` — used only by `dotnet ef` CLI commands, not at app runtime. |
| `Extensions/ServiceCollectionExtensions.cs` | `AddEasyEnglishRepositories()` — registers all 8 repositories. Does *not* register `IDbContextFactory<EasyEnglishDbContext>` or `IUserContext`; those come from `EasyEnglish.App`'s startup code. |
| `Extensions/LearningQueryExtensions.cs` | `ApplyLearningSelectionAsync` — shared "pick N items for a learning session" query logic, reused by `WordRepository`/`IrregularFormRepository`/`StudyCardRepository`/`TestCardRepository`. |
| `Extensions/NavigationQueryExtensions.cs` | `GetCyclicNavigationAsync` — shared cyclic prev/next index math, reused by `WordRepository`/`StudyCardRepository`/`TestCardRepository`'s `GetNavigationIdsAsync`. |
| `Repositories/*.cs` | One class per entity, each a thin `BaseRepository<T, EasyEnglishDbContext>`/`BaseWithGuidRepository<T, EasyEnglishDbContext>` subclass from `MukhaLab.Database`, adding only the domain queries declared on its `I*Repository` interface. |
| `Migrations/` | EF Core-generated. Not hand-documented here — see the current schema in `EasyEnglish.Docs/Diagrams/database.mdpuml`/`entities.mdpuml` instead of reading migration history. |

## Schema

The schema is entity-first: `EasyEnglish.Core.Entities.*`'s `[Table]`/`[Column]`/`[MaxLength]`/
`[ForeignKey]` attributes are the only source of truth. There is no fluent-API configuration, no
explicit indexes beyond what EF Core auto-creates for primary/foreign keys, and no unique
constraints (e.g. nothing stops two rows from sharing a `RecordGuid`). For the current 8-entity
model, see [`../EasyEnglish.Docs/Diagrams/entities.mdpuml`](../EasyEnglish.Docs/Diagrams/entities.mdpuml)
and [`database.mdpuml`](../EasyEnglish.Docs/Diagrams/database.mdpuml).

20 migrations so far, tracking a fair amount of schema churn (a `Dictionary`→`Course` rename, a
`WordList`→`Unit` rename, several card-kind/payload reshapes) — the migration folder is a useful
history if you need to understand *why* a column looks the way it does, but isn't summarized here
member-by-member.

## Repositories

Every repository follows the same shape: constructor takes `IDbContextFactory<EasyEnglishDbContext>`
and an optional `IUserContext` (defaulting to `null` — consistent across all 8) and opens a fresh
`DbContext` per method call via `contextFactory.CreateDbContextAsync()` — never a long-lived context
field. All read-only queries use `.AsNoTracking()`.

```csharp
public class WordRepository : BaseRepository<WordEntity, EasyEnglishDbContext>, IWordRepository
{
    public async Task<List<WordEntity>> GetByUnitAsync(int unitId, string[]? includes = null)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        IQueryable<WordEntity> query = ctx.Words.Where(w => w.UnitId == unitId);
        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);
        return await query.AsNoTracking().ToListAsync();
    }
}
```

CRUD, pagination, and filtering (`QueryParameters`) all come from `MukhaLab.Database`'s
`BaseRepository`/`BaseWithGuidRepository` — see that library's own README. What's added here per
entity:

- **`GetForLearningAsync`** (`Word`, `IrregularForm`, `StudyCard`, `TestCard`) — delegates to
  `LearningQueryExtensions.ApplyLearningSelectionAsync`, scoped to a course and optionally a unit.
- **`GetNavigationIdsAsync`** (`Word`, `StudyCard`, `TestCard` — *not* `IrregularForm`, confirmed
  intentional, see Known Issues #3) — cyclic prev/next lookup for a "swipe through items" UI, via
  the shared `NavigationQueryExtensions.GetCyclicNavigationAsync` helper.
- **`CountReviewedSinceAsync`** (`Word`, `IrregularForm`, `StudyCard`, `TestCard`) — for stats.
- One-off queries: `SubjectRepository.CountCoursesAsync`, `UnitRepository.GetByCourseAsync`/
  `GetUnitCardsAsync`, `ExampleRepository.GetByUnitAsync`, `WordRepository.GetNextWordsAsync`/
  `GetHardWordsAsync`.

`CourseRepository` adds nothing — it exists purely to give `IBaseWithGuidRepository<CourseEntity>`
a concrete, DI-registrable type.

## `LearningQueryExtensions.ApplyLearningSelectionAsync`

The shared selection algorithm behind every `GetForLearningAsync`. Reads `Rate`/`LastReviewDate`/
`CreatedAt` via `EF.Property<T>(entity, "PropertyName")` instead of direct interface member access
— accessing an interface member (`IRateInfo.Rate`) on a generic `T` doesn't reliably translate to
SQL across EF Core providers, while `EF.Property` is guaranteed to.

```csharp
var words = await ctx.Words
    .Where(w => w.Unit!.CourseId == courseId)
    .AsNoTracking()
    .ApplyLearningSelectionAsync(new LearningSelectionOptions
    {
        WordCount = 10,
        Priority = LearningPriority.Difficult,
    });
```

`LearningPriority.Random` is the one branch that can't stay in SQL — `ORDER BY RANDOM()` followed by
`Take` would need to be re-run per page and doesn't compose with the rest of this method's shape, so
that branch pulls the full filtered set into memory and shuffles with `Random.Shared` instead.

`LearningPriority.Old` spans *both* reviewed and never-reviewed items — unlike `New` (never-reviewed
only) and `Review` (reviewed only) — ranked by `COALESCE(LastReviewDate, CreatedAt)` ascending, so a
long-neglected never-reviewed item and a long-ago-reviewed item both count as "old" (see Known
Issues #5).

## Known Issues & Suggested Improvements

Found while documenting this library. Issues #1, #2, #4, and #5 below have since been fixed; #3 was
investigated and confirmed intentional. Kept here as a record of what changed and why.

1. ~~**`IUserContext` was required in 5 repository constructors, optional in 3.**~~ **Fixed.** All 8
   repository constructors now take `IUserContext? userContext = null`, matching
   `MukhaLab.Database.BaseRepository`'s own default. No behavior change — per-user row scoping still
   never activates anywhere in this app, since none of the 8 repositories call
   `ConfigureUserIdField(...)`.

2. ~~**`LearningQueryExtensions`'s "already learned" threshold (`1.6f`) didn't match
   `RateExtensions.EasyMax`**~~ **Fixed.** Now reads `RateExtensions.EasyMax` directly instead of a
   separately hand-typed `1.6f` — the two can no longer drift apart. (Behavior technically changed at
   the boundary: `1.6f` → `5f/3f ≈ 1.6667f`, a difference of `0.0667`; a word with `Rate` in that
   narrow band flips from "learned" to "not learned" for `IncludeLearnedWords: false`. Test updated
   to assert against the constant rather than the old literal.)

3. **`IrregularFormRepository`/`IIrregularFormRepository` has no `GetNavigationIdsAsync`**, unlike
   `Word`/`StudyCard`/`TestCard`. Investigated — this is a deliberate scope boundary (no prev/next
   swipe UI for irregular forms in `EasyEnglish.App`), not an oversight. Left as-is.

4. ~~**`GetNavigationIdsAsync`'s cyclic-navigation logic was duplicated near-identically three
   times**~~ **Fixed.** Extracted into `NavigationQueryExtensions.GetCyclicNavigationAsync` — each
   repository now supplies only its own filter/order/select (1 line), the shared helper does the
   index math. No behavior change (`GetNavigationIdsAsyncTests` unchanged, still green).

5. ~~**`LearningPriority.Old` and `LearningPriority.New` filtered identically**
   (`LastReviewDate == null`), differing only in sort direction.~~ **Fixed** (after initially being
   confirmed intentional and left as-is — the project owner reversed that call). `Old` now spans
   *both* reviewed and never-reviewed items, ordered by `COALESCE(LastReviewDate, CreatedAt)`
   ascending: a never-reviewed item's `CreatedAt` stands in for "last touched" when it has no
   `LastReviewDate`, so a word added long ago and still untouched, and a word last reviewed long ago,
   both surface as "old" — genuinely distinct from `New` (never-reviewed only, newest-first) and
   `Review` (reviewed only, oldest-review-first) instead of overlapping with `New`.

## Testing

`EasyEnglish.Data.Tests` (36 tests) uses a real SQLite in-memory `EasyEnglishDbContext` (one open
`SqliteConnection` per test class, same pattern as `MukhaLab.Database.Tests`) rather than EF Core's
`InMemory` provider — this project exists specifically to catch LINQ-to-SQL translation issues
(`EF.Property` usage, nested-navigation filters, static-constant references in `Where`/`Select`),
which the `InMemory` provider wouldn't reliably reproduce. Priority, per the same risk-based
framework used for `EasyEnglish.Core`:

- **`ApplyLearningSelectionAsync`** (highest risk) — all 5 `LearningPriority` branches plus the
  `IncludeLearnedWords` filter, executed against real SQLite to confirm the `EF.Property` calls
  actually translate and produce the right SQL, not just the right LINQ-to-Objects result.
- **`UnitRepository.GetUnitCardsAsync`** — confirms `RateExtensions.EasyMax`/`HardMin` (static
  readonly `float` fields referenced inside a `Select` projection) translate to SQL correctly.
- **`GetNavigationIdsAsync`** (`Word`/`StudyCard`/`TestCard`) — boundary cases: first item, last
  item, single-item unit, id not found in the unit.
- **`EasyEnglishDbContext.UpdateAuditInfo`** — `CreatedAt` set on insert, `UpdatedAt` set on update,
  via a real `SaveChangesAsync` round-trip.
- A representative sample of the remaining one-off queries (nested-navigation filters like
  `Example.Word.UnitId`, simple counts) for baseline coverage.

Not covered: `Migrations/` (generated, not hand-written logic) and `DesignTimeDbContextFactory`
(design-time-only, not exercised by the running app).
