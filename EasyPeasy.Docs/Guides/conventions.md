# Conventions & Gotchas

Things that aren't obvious from reading any single file, collected while documenting/testing/fixing
every `EasyPeasy.*`/`MukhaLab.*` project. If you're about to be surprised by something, check here
first — and if you find a new one, add it.

## Naming

- **Project names are plain English, not layer acronyms**: `Core` / `Data` / `Business` / `Cache` /
  `App`, not `DAL`/`BLL`. Deliberate — an acronym for one layer creates pressure to rename the
  others to match, and plain names read better once you're used to them.
- **`MukhaLab.*` is reserved for genuinely reusable, non-EasyPeasy-specific code** — no course/word/
  unit/English-grammar domain knowledge baked in. Something that's "not MAUI-coupled" but still
  encodes EasyPeasy's domain (e.g. `HomeStatsService`, `WordRatingCalculator`) does **not**
  qualify, even though it could technically compile in isolation.
- **Namespace trap**: `EasyPeasy.Services` (bare, no `.App` or `.Business` segment) is used by
  a handful of `EasyPeasy.App`-local classes (`FileService`, `CourseZipBackupService`) — it is
  **not** the same thing as `EasyPeasy.Business.Services` (the actual business/domain service
  layer, renamed from the historical `EasyPeasy.Services.Services`). The two are easy to conflate
  because they share the `EasyPeasy.Services` prefix; check the `using`/project, not just the
  short name, when you see `XyzService`.
- **`AddEasyPeasyDataServices()` lives in `EasyPeasy.Business`, not `EasyPeasy.Data`.** It
  registers the business/domain services (`WordService`, `UnitService`, etc.) — the name is a
  leftover and doesn't match its own project. `EasyPeasy.Data`'s equivalent method is
  `AddEasyPeasyRepositories()`.

## Language: code vs. UI text

- **All code comments, XML doc comments, and identifiers are English** — including on `internal`
  types (e.g. `TextChunkParser` in `EasyPeasy.App.Services.Speech`). No exceptions for
  visibility level.
- **UI-facing strings stay Ukrainian**: Razor component text, validation messages a learner sees,
  install-instruction strings shown in the voice-settings screen
  (`VoiceAvailabilityService.BuildInstructions`), file-picker dialog titles (`FileService`).
- **Exception messages are judged by where they end up**, not by a blanket rule: a message that's
  only ever logged/caught internally (never shown raw to the end user) is English — e.g.
  `CourseZipBackupService`'s parse-failure messages, `UnsupportedPronunciationCheckService`'s
  `PlatformNotSupportedException`. A message a learner could plausibly see unfiltered stays
  Ukrainian.

## Exceptions & error handling

- **Not-found paths throw `EntityNotFoundException`** (from `MukhaLab.Database`), not
  `NullReferenceException`, `ArgumentException`, or `InvalidOperationException`. This was
  inconsistent historically (`WordService.UpdateWordRateAsync` used to null-reference on a missing
  id) and has been standardized going forward — if you add a new not-found path, match this.
- **A row deleted out from under an update** (e.g. by a concurrent request) surfaces as
  `EntityNotFoundException` too — `MukhaLab.Database`'s repositories catch
  `DbUpdateConcurrencyException` and rewrap it, so callers only ever need to handle one exception
  type for "this thing doesn't exist (anymore)."
- **`null` on a child collection during a reconcile/update means "don't touch this collection,"
  never "delete everything in it."** Established after `UnitService.ReconcileAndUpdateAsync` was
  found to mass-delete a unit's words/examples/etc. when the incoming DTO's collection was `null`
  (which happens for real when reconciling from a partial backup-ZIP import). If you write a new
  reconcile-style method, treat `null` the same way — the reconcile methods now fill a `null`
  incoming collection from the existing entity before diffing.

## Dependency injection

- **A singleton that needs a scoped dependency must take `IServiceScopeFactory` and create a
  short-lived scope per operation** — not the scoped service directly (a "captive dependency," which
  silently freezes the scoped instance for the app's entire lifetime instead of getting a fresh one
  per logical operation). See `EasyPeasy.Cache`'s `WordCacheService`/`CurrentUnitCacheService` for
  the pattern.
- **`EasyPeasy.App/MauiProgram.cs` is the composition root** — the only place that references both
  `EasyPeasy.Data` and `EasyPeasy.Business` and wires the former's concrete repositories to the
  latter's interfaces. `EasyPeasy.Business` itself only depends on interfaces declared in
  `EasyPeasy.Core` (see [key-decisions.md](../Decisions/key-decisions.md)) — don't add a
  `EasyPeasy.Data` project reference to `EasyPeasy.Business` to "make it easier" to call a
  repository directly; go through the interface instead, or the layering breaks.

## Testing

- **xUnit everywhere.** `NSubstitute` where mocking an interface is the right call; a real
  in-memory SQLite connection (`Microsoft.Data.Sqlite` + EF Core's Sqlite provider) instead of EF
  Core's `InMemory` provider for anything that touches `DbContext` — `InMemory` doesn't enforce the
  same LINQ-to-SQL translation rules and will happily accept query shapes that fail against real
  SQLite.
- **A regression test encodes the exact previously-broken behavior**, not just "the fix works." When
  you fix a bug found during review, write the failing test first so it documents what was actually
  wrong.
- **Test through the public API, not by reflecting into private methods.** If a private method's
  logic is complex enough to need direct testing, that's usually a sign it should be `internal`
  and tested directly (with `InternalsVisibleTo`) instead of staying `private`.
- **Don't `ProjectReference` a MAUI head project (`UseMaui=true`, `OutputType=Exe`) from a test
  project.** It isn't supported by the MAUI SDK tooling — see
  [key-decisions.md](../Decisions/key-decisions.md) for the two independent ways this fails and the
  `<Compile Include>`-linking workaround `EasyPeasy.App.Tests` uses instead.
- **If a `dotnet build`/`dotnet test` involving `EasyPeasy.App`'s Windows target fails with a
  `StaticWebAssets.Publish.targets` `InvalidOperationException` about a `.gz` file's length not
  being defined**, that's a known flaky stale-cache issue, not a real problem — delete
  `EasyPeasy.App/obj` and `EasyPeasy.App/bin` and rebuild.

## Documentation

- **Every service/class gets an XML doc `<summary>`, including `internal`-visibility ones.** No
  "it's internal, nobody external sees it" exemption.
- **A "Known Issues & Suggested Improvements" section belongs in every project's README** — findings
  from documenting/testing a project that weren't fixed in that pass. Mark an entry `~~struck
  through~~` with **Fixed.** once it's addressed, rather than deleting it — it's a record of what
  changed and why, useful for anyone who finds the old GitHub issue/commit later.
