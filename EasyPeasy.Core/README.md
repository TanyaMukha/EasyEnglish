# EasyPeasy.Core

The domain layer of the EasyPeasy app: EF Core entities, UI/serialization-facing DTO models, the
AutoMapper profile that bridges them, repository/service contracts, and small supporting types
(enums, options, "includes" presets). This project has **no persistence logic and no framework
dependencies beyond EF Core's data-annotation attributes and AutoMapper** — it only defines shapes
and contracts. `EasyPeasy.Data` implements the repository interfaces against SQLite;
`EasyPeasy.Business` implements the service interfaces; `EasyPeasy.App` (MAUI Blazor Hybrid)
consumes the service interfaces and the `*Model` DTOs.

Only dependency: `MukhaLab.Database` (`AbstractEntity`, `AbstractModel`, `IGuidRecord`,
`IBaseRepository<T>`, `IBaseService<TModel>`, `QueryParameters`, ...) — see that library's own
README for the generic CRUD/query contracts every repository and service interface here builds on.

## Domain model

```
Subject 0..1───0..* Course 1───1..* Unit ─┬─1..* Word 1───0..* Example
                                          ├─1..* IrregularForm
                                          ├─1..* StudyCard
                                          └─1..* TestCard
```

For the full entity-relationship diagram (including columns) see
[`EasyPeasy.Docs/Diagrams/entities.mdpuml`](../EasyPeasy.Docs/Diagrams/entities.mdpuml) and
[`database.mdpuml`](../EasyPeasy.Docs/Diagrams/database.mdpuml).

A `Unit` is the unit of study: it owns four independent kinds of learnable content (`Word`,
`IrregularForm`, `StudyCard`, `TestCard`), each with its own review/rating state
(`IReviewInfo`/`IRateInfo`).

## Project layout

| Folder | Contents |
|---|---|
| `Entities/` | EF Core-mapped persistence classes (`[Table]`, `[Column]`, `[MaxLength]`, ...). Never leave `EasyPeasy.Data`. |
| `Models/` | DTOs (`AbstractModel`-derived) meant for the service layer, Blazor UI binding, and JSON (import/export, JS interop). `Models/TestCards/` holds the four typed payload shapes for `TestCardModel`. |
| `Mapping/` | The AutoMapper `Profile` plus custom `IMappingAction`/`ITypeConverter` classes bridging entities and models. |
| `Interfaces/Fields/` | Small "mixin" interfaces (`IAuditInfo`, `IGuidInfo`, `IRateInfo`, `IReviewInfo`) implemented by multiple entities/models. |
| `Interfaces/Repositories/` | Per-entity repository contracts, each extending `MukhaLab.Database`'s `IBaseRepository<T>` or `IBaseWithGuidRepository<T>`. |
| `Interfaces/Services/` | Per-model service contracts, each extending `IBaseService<TModel>` or `IBaseWithGuidService<TModel>`. |
| `Interfaces/Cache/`, `Interfaces/Storage/` | App-level caching/persistent-storage abstractions, unrelated to the SQLite database. |
| `Enums/` | Plain domain enums (`TestCardKind`, `StudyCardKind`, `PartOfSpeech`, `LanguageLevel`, `DifficultyLevel`, `LearningPriority`, `BlurRevealMode`). |
| `Extensions/` | `RateExtensions` — buckets a numeric rate into a `DifficultyLevel`. |
| `Options/` | `LearningSelectionOptions` — parameters for "pick N items for a learning session" queries. `UnitMergeOptions` — what an incoming unit graph is authoritative for, so a partial course archive can be merged without destroying data it doesn't carry (see `EasyPeasy.Business`'s `ReconcileAndUpdateAsync`). |
| `Presets/` | Named string arrays of EF Core `Include()` paths per entity (`*Includes.None`/`.Full`/...). |

## Entities vs. Models — why two parallel hierarchies

`*Entity` classes are the EF Core mapping surface: they carry `[Table]`/`[Column]`/`[MaxLength]`
attributes and EF navigation properties, and are only ever touched inside `EasyPeasy.Data`.
`*Model` classes are the DTOs that cross that boundary — returned by services, bound in Blazor
components, and serialized to JSON for import/export. Keeping them separate means the persistence
schema (column names, `[MaxLength]` limits) can change without touching UI-facing contracts, and
`[JsonIgnore]` can be applied on the model side (e.g. `WordModel.Unit`) to avoid serializing back-references without affecting the entity mapping at all.

`MappingProfile` (`AutoMapper.Profile`) is the only place that knows about both sides.

## Mapping

### Entity ↔ Model

Most maps are declarative `CreateMap<TSource, TDest>()` with `ForMember` overrides for navigation
properties. Two entities need more than that:

- **`TestCardEntity ↔ TestCardModel`** goes through a custom `ITypeConverter`
  (`TestCardEntityToModelConverter` / `TestCardModelToEntityConverter`) instead of `ForMember`,
  because the entity's `Options`/`CorrectAnswers` columns are opaque JSON whose shape depends on
  `TestCardKind`. The converter switches on `Kind` and packs/unpacks exactly one of
  `ChoicePayload`, `ShortAnswerPayload`, `ClozePayload`, `MatchingPayload` on the model side.

```csharp
// Entity -> Model: Kind picks which payload gets populated.
var model = mapper.Map<TestCardModel>(entity);
if (model.Kind == TestCardKind.SingleChoice)
{
    var options = model.Choice!.Options;
    var correct = model.Choice!.CorrectAnswers;
}
```

### Model ↔ Model (cloning with options)

`UnitModel → UnitModel`, `WordModel → WordModel`, and the equivalent self-maps for
`IrregularForm`/`StudyCard`/`TestCard`/`Example` exist for **cloning a unit** — e.g. duplicating it,
or reconciling an imported unit against an existing one. Plain AutoMapper would produce a literal
deep copy (same `Id`, same `RecordGuid`, same learning progress); `UnitMappingOptions` lets the
caller opt into resetting identity and/or excluding learning state, applied by `IMappingAction`
classes in `MappingActions.cs`:

```csharp
var clone = mapper.Map<UnitModel>(sourceUnit, opts =>
    opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
    {
        ResetId = true,          // Id/CourseId -> 0, timestamps refreshed
        RegenerateGuid = true,   // new RecordGuid
        Word = new WordMappingOptions
        {
            ResetId = true,
            ExcludeLearningProgress = true,  // Rate/LastReviewDate/ReviewCount reset
        },
    });
```

Without an entry in `ResolutionContext.Items` (i.e. a plain `mapper.Map<UnitModel>(source)`), every
`*MappingAction` is a no-op — see `MappingContextExtensions.GetUnitOptions`.

`IrregularForm`/`StudyCard`/`TestCard` options on `UnitMappingOptions` are wired to their own
independent `IMappingAction`s (`IrregularFormMappingAction`, etc.), applied when AutoMapper recurses
into `UnitModel.IrregularForms`/`StudyCards`/`TestCards` during the same `Map<UnitModel>()` call.

## Repositories and Services

Every `I*Repository`/`I*Service` interface adds only what's beyond generic CRUD — pagination,
filtering, and basic `Find`/`Update`/`Remove` all come from `MukhaLab.Database`. Typical additions
here are domain queries (`GetForLearningAsync`, `GetNavigationIdsAsync` for prev/next UI,
`CountReviewedSinceAsync` for stats) and batch rating updates (`UpdateRateRangeAsync`, using the
shared `UpdateWordRateRequest` DTO defined in `IWordService.cs`).

```csharp
public interface IWordRepository : IBaseRepository<WordEntity>
{
    Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
    // ...
}
```

## Presets

`Presets/*Includes` classes centralize the EF Core `Include()` path strings so callers don't
hand-type navigation paths at every call site:

```csharp
var units = await unitRepository.GetAsync(includes: UnitIncludes.Full);
// equivalent to Include("Words").Include("Course").Include("IrregularForms")
//              .Include("Words.Examples").Include("StudyCards").Include("TestCards")
```

These are plain strings, not compiler-checked against the entity's real navigation properties — see
[Known Issues](#known-issues--suggested-improvements).

## Known Issues & Suggested Improvements

Found while documenting this library. Issues #1–#4 below have since been fixed; #5 is left open by
design (see rationale). Kept here as a record of what changed and why, the same way
`MukhaLab.Database`'s README tracks its own fix history.

1. ~~**`UnitModel.ClearKeyFields()` / `ClearLearningProgress()` only touch `Words`.**~~ **Fixed.**
   Both methods now also reset `IrregularForms`, `StudyCards`, and `TestCards` the same way they
   reset `Words` (identity fields + timestamps for `ClearKeyFields`; `Rate`/`LastReviewDate`/
   `ReviewCount` for `ClearLearningProgress`). This was the highest-severity finding — a "silently
   wrong, no exception" gap that would corrupt data on unit cloning/import.

2. ~~**`IrregularFormModel.Unit` was typed as `UnitEntity`, not `UnitModel`.**~~ **Fixed.** Now
   `UnitModel?`, matching every sibling model. `MappingProfile`'s
   `CreateMap<IrregularFormEntity, IrregularFormModel>()` gained an explicit
   `.ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))`, matching the pattern
   already used for `Word`/`StudyCard`.

3. ~~**`UnitModel.LanguageCode` had no backing column.**~~ **Fixed.** Confirmed unused anywhere in
   the solution (only `CourseModel.LanguageCode`/`CourseEntity.LanguageCode` are real) and removed.

4. ~~**`CourseModel.cs` carried ~150 lines of commented-out code.**~~ **Fixed.** Removed
   `DictionaryStatus`, `DictionaryStatistics`, and the commented computed-property block; confirmed
   unused anywhere else in the solution before deleting.

5. **`Presets/*Includes` are unchecked strings.** Renaming a navigation property won't cause a
   compile error here — EF Core throws `ArgumentException` at runtime instead, and only when that
   specific preset actually executes. Left open: validating an `Include` path needs a real
   `DbContext`, which belongs in `EasyPeasy.Data`'s own test suite (this library has no `DbContext`
   of its own), not here.

6. ~~**`IrregularFormModel, IrregularFormModel` self-map didn't ignore `Unit`.**~~ **Fixed, found
   while fixing #2.** Once `IrregularFormModel.Unit` became `UnitModel?` (same type as the parent
   unit being cloned), AutoMapper's default convention would have started recursively self-mapping
   it too — unlike `Word`/`StudyCard`/`TestCard`, whose self-maps already `.Ignore()` `Unit`. Added
   the same `.Ignore()` for consistency, avoiding a latent recursive-mapping risk this fix would
   otherwise have introduced.

7. ~~**Five `CreateMap<UpdateWordRateRequest, X>()` maps failed `AssertConfigurationIsValid()`.**~~
   **Fixed, found while writing tests** (not by inspection — the exact pattern this project's testing
   guidelines predict). These are intentional partial-update maps (only `Id`/`Rate`/`LastReviewDate`/
   `ReviewCount` are meant to be set), but AutoMapper's default validation checks that every
   *destination* member is mapped. Switched to `CreateMap<UpdateWordRateRequest, X>(MemberList.Source)`,
   which validates that every *source* member is mapped instead — the correct semantics for a
   partial-update DTO, with no change to runtime mapping behavior.

## Testing

Most of this library is data shape with no behavior — entities, enums, interfaces, and simple
options/preset classes don't need dedicated tests (there's nothing to assert that the compiler
doesn't already guarantee). `EasyPeasy.Core.Tests` (50 tests) covers the pockets of real logic that do:

- **`TestCardConvertersTests`** — round-trip tests (Entity→Model→Entity and Model→Entity→Model) for
  all five `TestCardKind` values via `TestCardEntityToModelConverter`/`TestCardModelToEntityConverter`,
  including the `Cloze.Options == null` vs. `[]` distinction and an out-of-range `Kind` value hitting
  the converter's `default` branch. This converter is hand-written JSON pack/unpack with a `switch`
  per kind — the highest-risk logic in the library, since a wrong case shows a learner blank/garbled
  quiz options with no exception.
- **`MappingActionsTests`** — every `UnitMappingOptions` flag (`ResetId`, `RegenerateGuid`,
  `Word.ExcludeExamples`, `Word.ExcludeLearningProgress`, and the equivalents for
  `IrregularForm`/`StudyCard`/`TestCard`) exercised both on and off, plus confirms a plain
  `Map<UnitModel>(source)` with no options is a no-op (the `GetUnitOptions() is null` early return).
- **`UnitModelTests`** — `ClearKeyFields()`/`ClearLearningProgress()`/`RemoveExamples()` now assert
  all four child collections (`Words`, `IrregularForms`, `StudyCards`, `TestCards`), covering the
  fix for Known Issue #1.
- **`RateExtensionsTests`** — boundary tests at and around `EasyMax` (5/3) and `HardMin` (10/3),
  since the `<` cutoffs mean the exact boundary value belongs to the *next* bucket up.
- **`MappingProfileTests`** — a `configuration.AssertConfigurationIsValid()` smoke test. This alone
  caught Known Issue #7 (five `UpdateWordRateRequest` partial-update maps needed `MemberList.Source`)
  — a real, previously-undetected configuration gap found by writing the test, not by inspection.

Everything else — entities, enums, `Interfaces/*`, `Options/LearningSelectionOptions` — is state with
no logic and isn't worth dedicated unit tests. Presets (#5 above) are intentionally *not* covered
here; see the rationale above.
