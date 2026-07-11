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
(+ `IUserContext`, see [Known Issues](#known-issues--suggested-improvements) #1) and opens a fresh
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
- **`GetNavigationIdsAsync`** (`Word`, `StudyCard`, `TestCard` — *not* `IrregularForm`, see Known
  Issues #3) — cyclic prev/next lookup for a "swipe through items" UI.
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

## Known Issues & Suggested Improvements

Found while documenting this library. None have been changed.

1. **`IUserContext` is required in 5 repository constructors, optional (defaulting to `null`) in 3
   (`CourseRepository`, `UnitRepository`, `WordRepository`).** In practice this doesn't matter today:
   `EasyEnglish.App` registers `AnonymousUserContext` for `IUserContext` unconditionally, so every
   repository receives a non-null instance via DI regardless of which signature it uses — and *none*
   of the 8 repositories ever call `ConfigureUserIdField(...)`, so `BaseRepository.IsUserFilteringActive`
   stays `false` everywhere either way. Per-user row scoping is fully wired in `MukhaLab.Database` but
   completely inert in this app (expected — EasyEnglish is single-user/on-device). The inconsistency
   is only a latent footgun: if per-user scoping is ever turned on for one entity, there's no clear
   reason the constructor pattern differs today, so it's easy to miss updating the "optional" trio.

2. **`LearningQueryExtensions`'s "already learned" threshold (`1.6f`) doesn't match
   `RateExtensions.EasyMax`** (`5f / 3f ≈ 1.667`) — a different, hand-typed magic number expressing
   what looks like the same concept ("easy" / "already learned"). They currently happen to produce
   similar but not identical cutoffs; if `EasyMax` is ever retuned, this filter silently drifts out
   of sync with the `DifficultyLevel` bucketing shown in the UI.

3. **`IrregularFormRepository`/`IIrregularFormRepository` has no `GetNavigationIdsAsync`**, unlike
   `Word`/`StudyCard`/`TestCard`. May be intentional (no prev/next UI for irregular forms), but it's
   worth confirming — since the interface and implementation are already consistent with each other,
   this wouldn't surface as a compiler error or test failure either way.

4. **`GetNavigationIdsAsync`'s cyclic-navigation logic is duplicated near-identically three times**
   (`WordRepository`, `StudyCardRepository`, `TestCardRepository`) — same index math, same modulo
   wraparound, different entity type. A shared generic helper (parallel to how
   `ApplyLearningSelectionAsync` already centralizes the learning-selection logic) would remove the
   duplication and the risk of the three copies drifting apart.

5. **`LearningPriority.Old` and `LearningPriority.New` filter identically** (`LastReviewDate == null`)
   in `ApplyLearningSelectionAsync`, differing only in sort direction (`Old` ascending `CreatedAt`,
   `New` descending). But `LearningPriority.Old`'s own XML doc (in `EasyEnglish.Core`) says "not
   reviewed for the longest time" — wording that reads like it should target items that *were*
   reviewed, a long time ago (i.e. overdue, similar to `Review`), not items that were *never*
   reviewed at all (which is what `New` already covers). Found and pinned down as a regression test
   in `EasyEnglish.Data.Tests` (`Old_UsesSameNeverReviewedFilterAsNew_JustAscendingOrder`) — flagged
   here rather than "fixed" since which behavior is actually intended isn't clear from the code alone.

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
