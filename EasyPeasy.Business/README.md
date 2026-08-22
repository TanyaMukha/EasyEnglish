# EasyPeasy.Business

The service layer: implementations of `EasyPeasy.Core.Interfaces.Services.*`, sitting between
`EasyPeasy.App` (Blazor UI) and `EasyPeasy.Data` (repositories). Every service is a thin
`BaseService<TEntity, TModel>`/`BaseWithGuidService<TEntity, TModel>` subclass from
`MukhaLab.Database`, adding only the domain operations declared on its `I*Service` interface —
mapping entities to models, delegating to the matching `I*Repository`, and occasionally composing
other services.

## Project layout

| File | Purpose |
|---|---|
| `Extensions/ServiceCollectionExtensions.cs` | `AddEasyPeasyDataServices()` — registers all 8 services. Requires `AddEasyPeasyRepositories()` (from `EasyPeasy.Data`) and an `IMapper` registration to already be present; neither is done here. |
| `Services/SubjectService.cs`, `ExampleService.cs`, `IrregularFormService.cs`, `StudyCardService.cs`, `TestCardService.cs` | Straightforward — map to/from `MukhaLab.Database`'s CRUD, add one or two domain methods that delegate to the matching repository. |
| `Services/WordService.cs` | Adds review/rating update methods (`UpdateWordRateAsync`, `UpdateWordRateRangeAsync`) and navigation (`GetNavigationIdsAsync`). |
| `Services/CourseService.cs` | Composes `IUnitService`/`IWordService` — a course "contains" units/words only indirectly, through those services. |
| `Services/UnitService.cs` | The most complex file — composes all 5 child services and implements `ReconcileAndUpdateAsync`, a GUID-based reconciling update for a unit's full child graph. |

## `ReconcileAndUpdateAsync`

`IUnitService.ReconcileAndUpdateAsync(UnitModel incoming, UnitMergeOptions options, ...)` updates a
unit together with its children (`Words`/`Examples`/`IrregularForms`/`StudyCards`/`TestCards`),
matching each incoming child against the unit's *existing* children by `RecordGuid` instead of a
blind EF cascade by `Id`. This is what lets a course archive from another device — where every
local `Id` means something entirely different — be applied as an update instead of duplicating
everything or corrupting unrelated rows:

```csharp
var updated = await unitService.ReconcileAndUpdateAsync(importedUnit, new UnitMergeOptions
{
    DeleteMissing    = true,
    MergeExamples    = archive.Options.IncludeExamples,
    LearningProgress = LearningProgressMerge.PreferNewest,
});
```

**Identity is `RecordGuid`, never `Id`:**

- A child whose `RecordGuid` already exists among the unit's current children gets that row's real
  `Id` and is updated in place.
- A child with a `RecordGuid` not seen before is forced to `Id == 0` and inserted as new — whatever
  `Id` the caller supplied is discarded. Leaving a foreign `Id` in place makes EF treat the child as
  an existing row and throw `DbUpdateConcurrencyException` (or, on an ID collision, silently
  overwrite a stranger's row). See [key-decisions.md #11](../EasyPeasy.Docs/Decisions/key-decisions.md).

**Partial payloads** — the archive may deliberately not carry everything, and "absent" must not read
as "deleted" or "reset":

- A `null` child collection on `incoming` (or a matched word's `Examples`) means "this payload
  doesn't say anything about this collection" — left completely untouched, regardless of
  `DeleteMissing`. To actually clear every child of a kind, pass an explicit empty list (`[]`) with
  `DeleteMissing = true`.
- `MergeExamples = false` leaves every matched word's stored examples alone and ignores the incoming
  ones. Needed because an export that excluded examples still deserializes each word with an *empty*
  list, which would otherwise delete them all under `DeleteMissing`.
- `LearningProgress` decides whether incoming `Rate`/`LastReviewDate`/`ReviewCount` may win.
  `KeepExisting` for archives that don't carry progress (they deserialize to *defaults*, which would
  silently reset real progress); `PreferNewest` compares `LastReviewDate` per item so syncing between
  two devices never rolls progress backwards.
- `DeleteMissing` only ever applies to collections the payload is authoritative for — with
  `MergeExamples = false`, examples are never deleted no matter what it says.

FKs (`UnitId`/`WordId`) aren't touched by hand — EF assigns them when the graph is saved through
navigation collections in the underlying `UpdateAsync` call.

## Known Issues & Suggested Improvements

Found while documenting this library. All issues below have since been fixed. Kept here as a record
of what changed and why.

1. ~~**Project name (`EasyPeasy.Business`) didn't match its namespace
   (`EasyPeasy.Services.*`).**~~ **Fixed.** Renamed to `EasyPeasy.Business.Services`/
   `EasyPeasy.Business.Extensions` throughout, including `EasyPeasy.App`'s
   `using EasyPeasy.Services.Extensions;` (now `EasyPeasy.Business.Extensions`). Note:
   `EasyPeasy.App` also has its own, unrelated, bare `EasyPeasy.Services` namespace (for
   `UnitBackupOptions`/`CourseZipBackupService`/`FileService`) — that one was intentionally left
   untouched; it isn't part of this project.

2. ~~**`EasyPeasy.Business.csproj` referenced `EasyPeasy.Data.csproj` unused.**~~ **Fixed.**
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
   (`EasyPeasy.App/Components/Pages/ImportCourseZip.razor`) passes units deserialized from a
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

`EasyPeasy.Business.Tests` (29 tests). Risk-based priority, same framework as
`EasyPeasy.Core`/`EasyPeasy.Data`:

- **`UnitService.ReconcileAndUpdateAsync`** (highest priority) — GUID-matching across 5 child
  collections, both `DeleteMissing` branches, and the null-vs-explicit-empty-collection distinction
  (Known Issue #4) with a dedicated regression test for each. Plus the identity and partial-payload
  guarantees: a foreign `Id` on an unmatched child is zeroed (verified to throw
  `DbUpdateConcurrencyException` without the fix — a real crash, not a hypothetical),
  `MergeExamples = false` preserves stored examples against an empty incoming list, and each
  `LearningProgressMerge` branch is pinned including the "never-reviewed incoming can't erase stored
  history" edge case.
- **`WordService.UpdateWordRateAsync`** — happy path plus the not-found → `EntityNotFoundException` case.
- **`*.UpdateRateRangeAsync`** (`Word`/`IrregularForm`/`StudyCard`/`TestCard`) — ids that don't match
  an existing row are confirmed silently skipped, not reported.
- **`CourseService.GetWordsAsync`** — the `ShuffleWords` branch (order differs, membership doesn't).
- Baseline pass-through coverage for the remaining simple delegation methods.

Uses a real SQLite in-memory `EasyPeasyDbContext` behind the real repositories (same fixture
pattern as `EasyPeasy.Data.Tests`), not mocked repositories — services here are thin enough that
mocking the repository would mostly just test the mock.
