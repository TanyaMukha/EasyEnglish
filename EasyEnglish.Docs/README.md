# EasyEnglish.Docs

Solution-wide documentation: architecture diagrams, developer guides, and the log of key
architectural decisions. Audience is **developers** working on or extending this solution — not
end users of the EasyEnglish app itself.

Per-project documentation (file-by-file breakdown, Known Issues, Testing sections) lives in each
project's own `README.md` and is linked from here rather than duplicated — this project is the map
and the "why," not a copy of the per-project detail.

## Start here

New to this solution? Read in this order:

1. **[Guides/getting-started.md](Guides/getting-started.md)** — prerequisites, clone, build, run,
   test.
2. **[Diagrams/solution-architecture.mdpuml](Diagrams/solution-architecture.mdpuml)** — the layered
   architecture, one diagram.
3. **[Guides/solution-structure.md](Guides/solution-structure.md)** — what each project is, with
   links to its own README.
4. **[Guides/conventions.md](Guides/conventions.md)** — naming/language/DI/testing conventions and
   known gotchas. Read this before your first PR.
5. **[Decisions/key-decisions.md](Decisions/key-decisions.md)** — why things are built the way they
   are, for the decisions most likely to look "wrong" until you know the history.

## Diagrams

| File | What it shows |
|---|---|
| [solution-architecture.mdpuml](Diagrams/solution-architecture.mdpuml) | Layered architecture: `MukhaLab.*` infra → `Core` → `Data`/`Business` → `Cache` → `App`, annotated with the Dependency Inversion pattern between Business/Data/App. Start here for the big picture. |
| [project-dependencies.mdpuml](Diagrams/project-dependencies.mdpuml) | The exact `ProjectReference` graph, including test projects — ground truth, not a simplified narrative. |
| [entities.mdpuml](Diagrams/entities.mdpuml) | `EasyEnglish.Core` domain entities and their relationships. |
| [database.mdpuml](Diagrams/database.mdpuml) | SQLite table/column layout. |

Rendering these locally requires the Visual Studio "PlantUML Editor" extension configured with
Render Type = **Local** (Tools → Options → PlantUML → Advanced) — the default setting points at a
dead demo server and diagrams silently fail to render. See
[key-decisions.md #10](Decisions/key-decisions.md#10-plantuml-rendering-local-not-the-extensions-default-azure-demo-server).

## Guides

| File | Covers |
|---|---|
| [getting-started.md](Guides/getting-started.md) | Prerequisites, build, run, test — onboarding. |
| [solution-structure.md](Guides/solution-structure.md) | Every project, one paragraph each, linking to its own README. |
| [conventions.md](Guides/conventions.md) | Naming, language (code vs. UI text), exceptions, DI, testing, documentation conventions — and the gotchas that don't follow from reading any single file. |
| [testing-strategy.md](Guides/testing-strategy.md) | The risk-based framework used to decide what to test in every project, plus a coverage summary (397 tests across 9 `.Tests` projects as of the last full run). |

## Decisions

[Decisions/key-decisions.md](Decisions/key-decisions.md) — a lightweight ADR log. Ten entries
covering, among others: why `EasyEnglish.Business` has no project reference to `EasyEnglish.Data`;
why project names avoid `DAL`/`BLL`-style acronyms; what qualifies code for extraction into a
`MukhaLab.*` library; the DI captive-dependency fix pattern; the `EntityNotFoundException`
convention; how `null` is handled during reconcile operations; the `LearningPriority.Old` semantics
decision; and why `EasyEnglish.App.Tests` can't `ProjectReference` the MAUI app project.

## Per-project READMEs (not duplicated here)

- [EasyEnglish.Core/README.md](../EasyEnglish.Core/README.md)
- [EasyEnglish.Data/README.md](../EasyEnglish.Data/README.md)
- [EasyEnglish.Business/README.md](../EasyEnglish.Business/README.md)
- [EasyEnglish.Cache/README.md](../EasyEnglish.Cache/README.md)
- [EasyEnglish.App/Services/README.md](../EasyEnglish.App/Services/README.md)
