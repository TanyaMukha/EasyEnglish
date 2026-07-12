# Key Architectural Decisions

Lightweight ADR-style log of the decisions most likely to confuse a future contributor if left
unexplained — either because they're not obvious from reading the code alone, or because someone
could plausibly "fix" them back to the wrong state without knowing why they're the way they are.
Newest first isn't required; grouped by topic instead.

Each entry: **Context** (the problem), **Decision** (what was chosen), **Consequences** (what that
buys you / what it costs).

---

## 1. Business depends on Core's interfaces only, never on Data directly

**Context**: `EasyEnglish.Business` (the domain service layer — `WordService`, `UnitService`, etc.)
originally had a `ProjectReference` to `EasyEnglish.Data`, but nothing in its source actually used
any `EasyEnglish.Data` type — the reference was dead weight.

**Decision**: Removed the reference. `EasyEnglish.Business` depends only on repository/service
interfaces declared in `EasyEnglish.Core` (`IWordRepository`, `IUnitRepository`, etc.).
`EasyEnglish.Data` implements those interfaces concretely. `EasyEnglish.App`'s `MauiProgram.cs` is
the only place that references both projects, and it's where the concrete repositories get wired to
the interfaces Business expects (`AddEasyEnglishRepositories()` then `AddEasyEnglishDataServices()`).

**Consequences**: Classic Dependency Inversion — `EasyEnglish.Business` could be tested or reused
against an entirely different persistence layer without changing a line of business logic. The cost
is one extra hop to trace when reading code: a `WordService` method that "obviously" talks to a
database doesn't reference anything database-shaped directly, only an interface. See
[project-dependencies.mdpuml](../Diagrams/project-dependencies.mdpuml) for the exact graph this
produces.

---

## 2. Project names are plain English, not layer acronyms

**Context**: Considered renaming `EasyEnglish.Business` to something shorter/more classic like
`BLL` (business logic layer) to match a "DAL/BLL" naming scheme some .NET codebases use.

**Decision**: Kept plain-English names (`Core`, `Data`, `Business`, `Cache`, `App`). Rejected `BLL`.

**Consequences**: `BLL` is a dated convention that would create pressure to also rename `Data` to
`DAL` for consistency, and plain names are self-explanatory to anyone regardless of which .NET
era they learned conventions in. `Business` on its own is a perfectly standard 3-tier layer name —
the thing that actually needed fixing was the namespace mismatch (see #1's sibling issue below),
not the project name.

---

## 3. `MukhaLab.*` is reserved for genuinely reusable code

**Context**: While reviewing `EasyEnglish.App/Services/` for extraction candidates, several files
had "no MAUI dependency" but still encoded EasyEnglish's actual domain (course/unit/word concepts,
English-specific grammar handling) — e.g. `HomeStatsService`, `WordRatingCalculator`,
`PronunciationTextNormalizer`.

**Decision**: "No MAUI dependency" alone doesn't qualify something as reusable. Only code with *zero*
EasyEnglish-domain knowledge belongs under the `MukhaLab.*` prefix (matching the 4 existing
libraries: `SelectQueryParameters`, `Database`, `BrowserConsoleLogger`, `LoggerExtensionDelegate`,
none of which know anything about words, units, or courses).

**Consequences**: This is a naming/extraction *principle*, not yet an executed decision — no new
`MukhaLab.*` library has been created from `EasyEnglish.App` code. If that extraction happens later,
this is the bar a file needs to clear first.

---

## 4. Singleton services needing a scoped dependency use `IServiceScopeFactory`, not the scoped service directly

**Context**: `EasyEnglish.Cache`'s `WordCacheService`/`CurrentUnitCacheService` were registered
`AddSingleton` but took `IWordService`/`IUnitService` (registered `AddScoped` in
`EasyEnglish.Business`) directly in their constructors — a captive-dependency bug: the first-resolved
scoped instance gets frozen for the app's entire lifetime instead of getting a fresh one per logical
operation.

**Decision**: Both cache services now take `IServiceScopeFactory` and create a short-lived
`IServiceScope` per fetch, resolving `IWordService`/`IUnitService` from that scope.

**Consequences**: Correct even if the app starts creating additional DI scopes elsewhere in the
future (a background task, etc.) — today it's harmless in practice since `EasyEnglish.App` never
creates extra scopes, but the bug was latent, not theoretical. Any new singleton that needs a scoped
dependency should follow this same pattern.

---

## 5. `EntityNotFoundException` is the standard "this doesn't exist" exception

**Context**: Not-found handling was inconsistent across the stack —
`WordService.UpdateWordRateAsync` used to throw `NullReferenceException` via a lying `!`
null-forgiving operator over an actually-null value; `UnitService.ReconcileAndUpdateAsync` threw
`ArgumentException`.

**Decision**: Every not-found path throws `MukhaLab.Database`'s `EntityNotFoundException`, including
the case where a row is deleted out from under an in-flight update (caught as
`DbUpdateConcurrencyException` and rewrapped).

**Consequences**: A caller only needs to catch one exception type for "this thing doesn't exist,
whether it never did or was deleted since you loaded it." Any new not-found path should match this.

---

## 6. `null` on a child collection means "don't touch it," never "delete everything"

**Context**: `UnitService.ReconcileAndUpdateAsync` (GUID-based reconcile of a unit's word/example/
irregular-form/study-card/test-card collections) treated a `null` incoming collection as "delete
every existing item of that kind" when `deleteMissing: true`. The one real call site
(`ImportCourseZip.razor`) passes units deserialized from a backup-ZIP's JSON, which can plausibly
omit a child array — confirmed by defensive `?.Count ?? 0` code already present elsewhere in
`CourseZipBackupService`. This made a partial/older backup silently wipe data on import.

**Decision**: A `null` incoming child collection is now filled from the *existing* entity's
collection before diffing — so `null` behaves as "no opinion, leave it alone," and only an
explicit (possibly empty) list can add/remove/delete items.

**Consequences**: Importing an older or partial backup ZIP can no longer silently delete data it
simply didn't know about. If you write a new reconcile-style method anywhere, treat `null` the
same way.

---

## 7. `LearningPriority.Old` spans both reviewed and never-reviewed items

**Context**: `LearningPriority.Old`'s filter was byte-for-byte identical to `LearningPriority.New`'s
(`LastReviewDate == null`), which didn't match its own XML doc or the UI labels ("Найстаріші"/"Давно
не повторювались"). Genuinely ambiguous — not a bug with one obvious right answer, since New/Old
being a matched pair over the same never-reviewed pool was also a defensible reading.

**Decision**: After being raised twice (once answered "leave as-is," reversed in a later
conversation with no new evidence), `Old` was changed to rank by
`COALESCE(LastReviewDate, CreatedAt)` ascending — spanning *every* item, ordered by whichever
timestamp reflects "last touched." A long-neglected never-reviewed item and a long-ago-reviewed item
now both surface as "old."

**Consequences**: `New` and `Old` are no longer a matched pair over the same pool — `New` still only
covers never-reviewed items; `Old` now covers everything. If this reads as surprising later, that's
the history: it was a judgment call on ambiguous product intent, not a mechanical bug fix.

---

## 8. `EasyEnglish.App.Tests` compiles source files directly instead of `ProjectReference`-ing the MAUI app

**Context**: `EasyEnglish.App` is a MAUI head project (`UseMaui=true`, `OutputType=Exe`,
`MauiIcon`/`MauiSplashScreen`/`MauiImage` resizetizer items, self-contained Windows packaging).
Referencing it as a `ProjectReference` from a test project — the pattern every other `.Tests`
project in this solution uses — failed in two independent, unrelated ways: a `StaticWebAssets`
compression step (`InvalidOperationException: File length ... is not defined`) and, once past that,
the Resizetizer's duplicate-icon detection (`Microsoft.Maui.Resizetizer.After.targets`: "duplicate
file names detected... appicon"). Neither is a cache/config issue — referencing an app-type MAUI
project from another project isn't supported by this tooling.

**Decision**: `EasyEnglish.App.Tests` targets plain `net9.0` (no MAUI) with **no** `ProjectReference`
to `EasyEnglish.App`. Instead it pulls in the ~10 pure-logic `.cs` files it needs directly via
`<Compile Include="...\EasyEnglish.App\..." Link="..." />` — the real production source, not a
copy — plus a normal `ProjectReference` to `EasyEnglish.Core.csproj` for the model types those files
depend on.

**Consequences**: Sidesteps the MAUI toolchain entirely rather than fighting it; edits to the linked
files are automatically picked up by the tests since it's the same source, not a copy. The
alternative — extracting the pure-logic files into a new class library project — was deliberately
not taken here, since that's a real structural change and a decision (see #3) that hadn't been
opted into. If `EasyEnglish.App/Services/` ever *does* get split into a `MukhaLab.*`-style library,
this test project's `<Compile Include>` list is exactly the set of files that would move.

---

## 9. Real SQLite in-memory over EF Core's `InMemory` provider

**Context**: Needed a way to test EF Core repository/query logic without a real database server.

**Decision**: Every test touching a `DbContext` (`EasyEnglish.Data.Tests`,
`EasyEnglish.Business.Tests`'s integration fixtures, `MukhaLab.Database.Tests`) uses a real
`Microsoft.Data.Sqlite` connection (`Data Source=:memory:`) through EF Core's Sqlite provider, not
the `Microsoft.EntityFrameworkCore.InMemory` package.

**Consequences**: `InMemory` doesn't enforce the same LINQ-to-SQL translation rules as a real
relational provider — it will happily accept query shapes that compile against `InMemory` but throw
or behave differently against real SQLite (which is exactly the class of bug several test suites
exist to catch, e.g. `MukhaLab.SelectQueryParameters`' filter expressions). The cost is slightly
more test-fixture setup (`IAsyncLifetime` to open/close a connection) versus `InMemory`'s
zero-config feel.

---

## 10. PlantUML rendering: Local, not the extension's default Azure demo server

**Context**: PlantUML diagrams in this project (`.mdpuml` files under `EasyEnglish.Docs/Diagrams/`)
silently failed to render in Visual Studio.

**Decision**: The root cause was the "PlantUML Editor" VS extension's default Render Type pointing
at a dead Azure demo server. Fixed via Tools → Options → PlantUML → Advanced → Render Type =
**Local**.

**Consequences**: Anyone opening this repo for the first time and finding diagrams don't render
should check this setting before assuming the `.mdpuml` files themselves are broken.
