# EasyEnglish.Business

The service layer: implementations of `EasyEnglish.Core.Interfaces.Services.*`, sitting between
`EasyEnglish.App` (Blazor UI) and `EasyEnglish.Data` (repositories). Every service is a thin
`BaseService<TEntity, TModel>`/`BaseWithGuidService<TEntity, TModel>` subclass from
`MukhaLab.Database`, adding only the domain operations declared on its `I*Service` interface —
mapping entities to models, delegating to the matching `I*Repository`, and occasionally composing
other services.

**Naming note:** the project/folder/assembly is named `EasyEnglish.Business`, but every file in it
uses the C# namespace `EasyEnglish.Services.*` (`EasyEnglish.Services.Services`,
`EasyEnglish.Services.Extensions`) — not `EasyEnglish.Business.*`. `EasyEnglish.App` already
references it via `using EasyEnglish.Services.Extensions;`, so this is baked into how consumers use
the project, not just an internal quirk. See [Known Issues](#known-issues--suggested-improvements) #1.

## Project layout

| File | Purpose |
|---|---|
| `Extensions/ServiceCollectionExtensions.cs` | `AddEasyEnglishDataServices()` — registers all 8 services. Requires `AddEasyEnglishRepositories()` (from `EasyEnglish.Data`) and an `IMapper` registration to already be present; neither is done here. |
| `Services/SubjectService.cs`, `ExampleService.cs`, `IrregularFormService.cs`, `StudyCardService.cs`, `TestCardService.cs` | Straightforward — map to/from `MukhaLab.Database`'s CRUD, add one or two domain methods that delegate to the matching repository. |
| `Services/WordService.cs` | Adds review/rating update methods (`UpdateWordRateAsync`, `UpdateWordRateRangeAsync`) and navigation (`GetNavigationIdsAsync`). |
| `Services/CourseService.cs` | Composes `IUnitService`/`IWordService` — a course "contains" units/words only indirectly, through those services. |
| `Services/UnitService.cs` | The most complex file — composes all 5 child services and implements `ReconcileAndUpdateAsync`, a GUID-based reconciling update for a unit's full child graph. |

## `ReconcileAndUpdateAsync`

`IUnitService.ReconcileAndUpdateAsync(UnitModel incoming, bool deleteMissing, ...)` updates a unit
together with its children (`Words`/`Examples`/`IrregularForms`/`StudyCards`/`TestCards`), matching
each incoming child against the unit's *existing* children by `RecordGuid` instead of a blind EF
cascade by `Id`. This is what makes "import a unit you exported earlier, but some IDs got reset to 0
during round-tripping" behave as an update instead of duplicating everything:

```csharp
var updated = await unitService.ReconcileAndUpdateAsync(importedUnit, deleteMissing: true);
```

- A child whose `RecordGuid` already exists among the unit's current children gets that row's real
  `Id` and is updated in place.
- A child with a `RecordGuid` not seen before keeps `Id == 0` and is inserted as new.
- `deleteMissing: true` additionally deletes any existing child whose `RecordGuid` isn't present in
  `incoming` — a strict sync. `deleteMissing: false` only adds/updates, never deletes.

FKs (`UnitId`/`WordId`) aren't touched by hand — EF assigns them when the graph is saved through
navigation collections in the underlying `UpdateAsync` call.

## Known Issues & Suggested Improvements

Found while documenting this library. None have been changed.

1. **Project name (`EasyEnglish.Business`) doesn't match its namespace (`EasyEnglish.Services.*`)**
   — see the naming note above. Purely cosmetic (compiles and runs fine either way), but confusing
   for anyone navigating by "what namespace is X in" instead of "what folder is X in."

2. **`EasyEnglish.Business.csproj` references `EasyEnglish.Data.csproj`, but nothing in the project
   actually uses any `EasyEnglish.Data` type.** The only trace of why the reference might exist —
   `using EasyEnglish.Data.Repositories;` in `UnitService.cs` and `WordService.cs` — was an unused
   import, now removed. A service layer depending on the concrete persistence layer at compile time
   (rather than only on `EasyEnglish.Core`'s interfaces, with `EasyEnglish.Data` wired in only via DI
   at the `EasyEnglish.App` composition root) blurs the intended layering; the `ProjectReference`
   itself is still there and could likely be removed.

3. **`WordService.UpdateWordRateAsync` throws `NullReferenceException` instead of a clear
   not-found signal.** When `word.Id` doesn't match an existing word, `GetByIdAsync` returns `null`,
   but the method still does `model!.Id` — the `!` doesn't null-check, it just silences the compiler.
   Every other "not found" path in this codebase (`MukhaLab.Database`'s `BaseRepository`/`BaseService`)
   was unified under `EntityNotFoundException` earlier this session; this method predates/bypasses
   that convention. **Highest-severity finding** — an NRE crash is a worse failure mode for callers
   than a typed exception they can catch and handle (e.g. show "word not found" instead of a generic
   error).

4. **`ReconcileAndUpdateAsync` can silently mass-delete a unit's children.** If a caller passes an
   `incoming` `UnitModel` whose `Words` (or `Examples`/`IrregularForms`/`StudyCards`/`TestCards`) is
   `null` — the default for a freshly-constructed `UnitModel`, not an explicit "empty the list"
   signal — combined with `deleteMissing: true`, `FindOrphanIds` treats *every* existing child of
   that kind as orphaned and deletes it. A `null` collection and an intentionally-emptied collection
   are indistinguishable to this method. Anyone calling this without populating every child
   collection they don't intend to touch, while passing `deleteMissing: true`, risks silent,
   unrecoverable data loss.

5. **`ReconcileAndUpdateAsync` throws a plain `ArgumentException` when the unit isn't found**
   (`"Unit with ID {id} not found"`), not `EntityNotFoundException` like the rest of the codebase's
   not-found paths (see #3 — the same underlying inconsistency in two places).

6. **Inconsistent `try`/`catch`/log/rethrow usage.** `WordService.UpdateWordRateRangeAsync`/
   `GetNavigationIdsAsync`, `IrregularFormService.UpdateRateRangeAsync`, and the equivalent
   `StudyCardService`/`TestCardService` methods wrap their bodies in `try { ... } catch (Exception ex)
   { _logger.LogError(...); throw; }` (log then rethrow unchanged) — but `SubjectService`,
   `ExampleService`, `CourseService`, and most of `UnitService` don't. Since the exception always
   propagates either way, this only affects whether an error gets one extra structured log entry
   before bubbling up — worth a consistent rule (always, or only at a single boundary layer) rather
   than per-method judgment calls.

7. **`WordService.cs` had a ~15-line commented-out earlier draft of `UpdateWordRateRangeAsync`**
   sitting directly above the real, working implementation — removed during this documentation pass
   (confirmed dead: the live method below it is what's actually registered/called).

## Testing

`EasyEnglish.Business.Tests` (19 tests). Risk-based priority, same framework as
`EasyEnglish.Core`/`EasyEnglish.Data`:

- **`UnitService.ReconcileAndUpdateAsync`** (highest priority) — GUID-matching across 5 child
  collections, both `deleteMissing` branches, and specifically the null-collection mass-delete risk
  (#4 above) as a documented regression test, not a silent assumption.
- **`WordService.UpdateWordRateAsync`** — including a regression test that pins down the current
  `NullReferenceException` behavior on a not-found id (#3 above), so a future fix has something to
  update rather than a behavior nobody wrote down.
- **`*.UpdateRateRangeAsync`** (`Word`/`IrregularForm`/`StudyCard`/`TestCard`) — ids that don't match
  an existing row are silently skipped; worth confirming that's what actually happens.
- **`CourseService.GetWordsAsync`** — the `ShuffleWords` branch (order differs, membership doesn't).
- Baseline pass-through coverage for the remaining simple delegation methods.

Uses a real SQLite in-memory `EasyEnglishDbContext` behind the real repositories (same fixture
pattern as `EasyEnglish.Data.Tests`), not mocked repositories — services here are thin enough that
mocking the repository would mostly just test the mock.
