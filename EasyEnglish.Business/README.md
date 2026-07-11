# EasyEnglish.Business

The service layer: implementations of `EasyEnglish.Core.Interfaces.Services.*`, sitting between
`EasyEnglish.App` (Blazor UI) and `EasyEnglish.Data` (repositories). Every service is a thin
`BaseService<TEntity, TModel>`/`BaseWithGuidService<TEntity, TModel>` subclass from
`MukhaLab.Database`, adding only the domain operations declared on its `I*Service` interface —
mapping entities to models, delegating to the matching `I*Repository`, and occasionally composing
other services.

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
- A `null` child collection on `incoming` (or a matched word's `Examples`) means "this payload
  doesn't say anything about this collection" — it's left completely untouched, regardless of
  `deleteMissing`. To actually clear every child of a kind, pass an explicit empty list (`[]`) with
  `deleteMissing: true`.

FKs (`UnitId`/`WordId`) aren't touched by hand — EF assigns them when the graph is saved through
navigation collections in the underlying `UpdateAsync` call.

## Known Issues & Suggested Improvements

Found while documenting this library. All issues below have since been fixed. Kept here as a record
of what changed and why.

1. ~~**Project name (`EasyEnglish.Business`) didn't match its namespace
   (`EasyEnglish.Services.*`).**~~ **Fixed.** Renamed to `EasyEnglish.Business.Services`/
   `EasyEnglish.Business.Extensions` throughout, including `EasyEnglish.App`'s
   `using EasyEnglish.Services.Extensions;` (now `EasyEnglish.Business.Extensions`). Note:
   `EasyEnglish.App` also has its own, unrelated, bare `EasyEnglish.Services` namespace (for
   `UnitBackupOptions`/`CourseZipBackupService`/`FileService`) — that one was intentionally left
   untouched; it isn't part of this project.

2. ~~**`EasyEnglish.Business.csproj` referenced `EasyEnglish.Data.csproj` unused.**~~ **Fixed.**
   `ProjectReference` removed.

3. ~~**`WordService.UpdateWordRateAsync` threw `NullReferenceException` instead of a clear
   not-found signal.**~~ **Fixed.** Now throws `EntityNotFoundException`, matching the rest of the
   codebase's not-found convention.

4. ~~**`ReconcileAndUpdateAsync` could silently mass-delete a unit's children** when an incoming
   child collection was `null`.~~ **Fixed.** A `null` collection (on `incoming` or on a matched
   word's `Examples`) is now filled in from the unit's current state before reconciliation runs, so
   it's treated as "don't touch this collection," never as "empty it out." An explicit empty list
   (`[]`) still triggers full deletion under `deleteMissing: true`. This was a real, reachable bug,
   not just a theoretical one: the one production call site
   (`EasyEnglish.App/Components/Pages/ImportCourseZip.razor`) passes units deserialized from a
   backup ZIP's JSON, which can plausibly omit a child array entirely.

5. ~~**`ReconcileAndUpdateAsync` threw a plain `ArgumentException` when the unit isn't found.**~~
   **Fixed.** Now throws `EntityNotFoundException`, matching #3's fix and the rest of the codebase.

6. ~~**Inconsistent `try`/`catch`/log/rethrow usage.**~~ **Fixed.** Removed the redundant
   `try { ... } catch (Exception ex) { _logger.LogError(...); throw; }` wrappers (they only added a
   log entry before rethrowing the same exception unchanged) from `WordService`/`IrregularFormService`/
   `StudyCardService`/`TestCardService`, matching the rest of the codebase's plain (unwrapped) style.

7. ~~**`WordService.cs` had a ~15-line commented-out earlier draft of `UpdateWordRateRangeAsync`.**~~
   **Fixed.** Removed during the documentation pass (confirmed dead).

## Testing

`EasyEnglish.Business.Tests` (21 tests). Risk-based priority, same framework as
`EasyEnglish.Core`/`EasyEnglish.Data`:

- **`UnitService.ReconcileAndUpdateAsync`** (highest priority) — GUID-matching across 5 child
  collections, both `deleteMissing` branches, and the null-vs-explicit-empty-collection distinction
  (Known Issue #4) with a dedicated regression test for each.
- **`WordService.UpdateWordRateAsync`** — happy path plus the not-found → `EntityNotFoundException` case.
- **`*.UpdateRateRangeAsync`** (`Word`/`IrregularForm`/`StudyCard`/`TestCard`) — ids that don't match
  an existing row are confirmed silently skipped, not reported.
- **`CourseService.GetWordsAsync`** — the `ShuffleWords` branch (order differs, membership doesn't).
- Baseline pass-through coverage for the remaining simple delegation methods.

Uses a real SQLite in-memory `EasyEnglishDbContext` behind the real repositories (same fixture
pattern as `EasyEnglish.Data.Tests`), not mocked repositories — services here are thin enough that
mocking the repository would mostly just test the mock.
