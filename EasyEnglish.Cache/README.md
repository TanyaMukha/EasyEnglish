# EasyEnglish.Cache

An in-memory "working set" cache layer for `EasyEnglish.App` — not a general-purpose caching
library. It caches a small, explicitly-selected set of entities (the current unit; the words in
the active learning session) whose *membership* persists across app restarts via
`EasyEnglish.Core.Interfaces.Storage.IStorageService`, so the app can restore "what was selected"
on next launch. It is not an expiry/LRU cache and has no size limit or eviction policy — entries
stay until explicitly removed.

## Project layout

| File | Purpose |
|---|---|
| `Extensions/ServiceCollectionExtensions.cs` | `AddEasyEnglishCacheServices()` — registers `ICurrentUnitCacheService`/`IWordCacheService` as singletons. |
| `Services/BaseCacheService.cs` | Generic multi-entity cache (`Dictionary<TId, TEntity>` + a persisted list of selected ids). Backs `WordCacheService`. |
| `Services/BaseSingleCacheService.cs` | Generic single-entity cache (one selected id, one cached entity). Backs `CurrentUnitCacheService`. |
| `Services/WordCacheService.cs` | Concrete `BaseCacheService<WordModel, int>`, backed by `IWordService`. |
| `Services/CurrentUnitCacheService.cs` | Concrete `BaseSingleCacheService<UnitModel, int>`, backed by `IUnitService`. |

Both base classes follow the same shape: a `SelectedId(s)` list/value persisted via
`IStorageService`, a `Cache`/`CachedEntity` in-memory store, and lazy `InitializeAsync()` (guarded
by a `SemaphoreSlim` so concurrent first calls don't double-fetch) that every other public method
calls before touching state.

```csharp
public class WordCacheService : BaseCacheService<WordModel, int>, IWordCacheService
{
    protected override string StorageKey => "selectedWordIds";

    protected override async Task<List<WordModel>> FetchEntitiesAsync(List<int> ids)
    {
        using var scope = _scopeFactory.CreateScope();
        var wordService = scope.ServiceProvider.GetRequiredService<IWordService>();
        return await wordService.GetByIdsAsync(ids);
    }

    protected override int GetEntityId(WordModel entity) => entity.Id;
}
```

## Known Issues & Suggested Improvements

Found while documenting this library. Issues #1 and #2 below have since been fixed. Kept here as a
record of what changed and why.

1. ~~**`AddSingleton<ICurrentUnitCacheService>`/`AddSingleton<IWordCacheService>` depended on
   `IUnitService`/`IWordService` directly**, which `EasyEnglish.Business` registers `AddScoped` — a
   classic DI lifetime mismatch ("captive dependency").~~ **Fixed.** Both cache services now take an
   `IServiceScopeFactory` instead, and create a short-lived scope per fetch to resolve
   `IUnitService`/`IWordService` from — the standard pattern for a singleton that needs a scoped
   dependency. No functional change (the app never created additional scopes, so the captured
   instance was already the only one that existed), but it removes the latent risk: this now behaves
   correctly even if the app ever starts creating scopes elsewhere (a background task, a port toward
   ASP.NET Core-style hosting), and won't throw under DI container configurations that validate scopes.

2. ~~**A repeated `AddAsync`/`SetAsync` call for an id that's already selected never refreshed the
   cached entity.**~~ **Fixed.** Both methods now always (re)fetch the entity, even when the id was
   already selected — only the persisted-selection write is still skipped in that case (to avoid a
   redundant duplicate-free-list write, and in `BaseCacheService`'s case, to avoid duplicate entries
   in `SelectedIds`). A stale cached copy can no longer linger just because it was added/selected once
   before; the previous "call `ClearCache()` to force a refetch" workaround is no longer needed for
   this specific case.

3. **`BaseSingleCacheService.SetAsync` isn't guarded by the `_initLock`** (unlike `InitializeAsync`).
   Two concurrent `SetAsync` calls for different ids can interleave: `SelectedId`/`CachedEntity` are
   read and written outside any lock, so it's possible for `SelectedId` to end up reflecting one call
   while `CachedEntity` reflects the other, if their `FetchEntityAsync` calls complete out of order.
   `BaseCacheService.AddAsync` has an analogous unguarded read/write pattern, though its dictionary-add
   shape makes the failure mode less severe (worst case: a wasted duplicate fetch, not a mismatched pair).

4. **`BaseSingleCacheService.GetEntityId` is declared abstract but never called anywhere in the base
   class.** Both concrete subclasses still have to implement it (to satisfy the abstract member), but
   it does nothing there — dead API surface, likely copy-pasted from `BaseCacheService` (where the
   equivalent method *is* used, to index the dictionary) without checking whether the single-entity
   version actually needed it.

## Testing

`EasyEnglish.Cache.Tests` (19 tests). No SQLite needed — this library has no EF Core/database
dependency, so `IUnitService`/`IWordService` are mocked with NSubstitute. `IStorageService` is a
hand-rolled fake that round-trips values through real JSON serialization rather than holding onto
object references — an early version that just stored references caused a false test failure in
`ClearCache_...` (mutating the cache's in-memory list also mutated "storage", since they were the
same object) that a real Preferences/SecureStorage-backed implementation can't actually exhibit
(deserializing always produces a fresh object). A naive in-memory fake would have hidden that gap
between test and production behavior instead of surfacing it. `IServiceScopeFactory` is a *real*
`ServiceCollection`-backed instance (via `TestScopeFactory.ForInstance`), not mocked — exercising the
actual scope-per-fetch resolution mechanism from Known Issue #1's fix, not just a stand-in for it.
Priority, per the same risk-based framework used elsewhere in this codebase:

- **Lazy `InitializeAsync` semantics** — loads from storage exactly once; a second call is a no-op;
  `ClearCache()` makes the next call re-initialize from (unchanged) persisted storage.
- **The same-id-always-refetches fix** (Known Issue #2) — `AddAsync`/`SetAsync` with an
  already-selected id now triggers a second fetch (not a no-op), while `AddAsync` still avoids adding
  a duplicate entry to the persisted selection.
- **CRUD-equivalent behavior**: `GetAllAsync`/`GetByIdAsync`/`ContainsAsync` against cached vs.
  uncached ids; `AddAsync` fetching and caching a new id; `RemoveAsync` dropping both the selection
  and the cached entity.
- Baseline coverage for `CurrentUnitCacheService`/`WordCacheService` wiring their `StorageKey`/
  `FetchEntit*Async` correctly to the underlying service (through a real scope resolution).

Not covered: the `SetAsync` race (Known Issue #3), since reliably reproducing a specific interleaving
requires either flaky timing-based tests or complicating the production code with test-only hooks,
neither of which is worth it for a scenario that would need real concurrent callers to trigger in the
first place (the app is single-user/single-window).
