# EasyEnglish.ContentTools

A personal console tool for authoring course content — not part of the running app, and not
referenced by any other project except `EasyEnglish.Core` (for the model types it edits). Run with
`dotnet run -- <module-key>` from this directory; with no arguments, the most recently added module
runs by default (see the `moduleKey` switch at the top of `Program.cs`).

This isn't scaffolding or a prototype: it edits real course-ZIP archives
(`C:\Users\User\Downloads\english_for_it_b2_*.zip`) that get imported straight into the running
EasyEnglish app through its own "Import Course ZIP" feature
(`EasyEnglish.App/Components/Pages/ImportCourseZip.razor`, backed by
`EasyEnglish.App/Services/CourseZipBackupService.cs`). `CourseZipEditor.JsonOpts` is deliberately
kept byte-for-byte in sync with `CourseZipBackupService.JsonOpts` — mismatched settings there would
produce a JSON file the app can't read back.

## Project layout

| File | Purpose |
|---|---|
| `Program.cs` | Top-level entry point: dispatches to one course-module class per `moduleKey`, plus a `verify` command that prints a summary of a unit's card counts. Each module class is a self-contained, one-off content-authoring run — add a new class here (and a new `case` in the switch) for the next course module. |
| `CourseZipEditor.cs` | Low-level ZIP editing: load/save a single `unit_N.json` or `course.json` inside an archive without disturbing the rest of it (words, audio, other units). |
| `CardBuilders.cs` | `TestCardBuilder`/`StudyCardBuilder` factory methods — one per `TestCardKind`/`StudyCardKind` — so a module class doesn't need to hand-assemble `Kind` + the right payload type every time. |

## How a module run actually works

Each course-module class (`EnglishForItB2Unit1`, `EnglishForItB2PrepositionsOfPlace`, etc.) follows
the same shape: copy the source archive to a new target file (never edit in place), load the unit
JSON out of it, append `StudyCard`/`TestCard` entries built via `CardBuilders`, save the unit back,
and — only when adding a brand-new unit file rather than appending to an existing one — also patch
`course.json`'s manifest so the app's importer knows the new unit file exists at all.

```csharp
internal static class EnglishForItB2Unit1
{
    private const string SourceZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated.zip";
    private const string TargetZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated2.zip";
    private const string UnitFile = "units/unit_1.json";

    public static void Run()
    {
        CourseZipEditor.CopyArchive(SourceZip, TargetZip);
        var unit = CourseZipEditor.LoadUnit(TargetZip, UnitFile);
        // ...append StudyCard/TestCard entries via CardBuilders...
        CourseZipEditor.SaveUnit(TargetZip, UnitFile, unit);
    }
}
```

`Id`/`UnitId` on every card built this way are left at `0` — on import, EF assigns the real foreign
key itself via cascaded insert of the parent `Unit` (see `CardBuilders.cs`'s class doc).

## Design notes

- **Hardcoded absolute paths, on purpose.** Every module class points at specific files under
  `C:\Users\User\Downloads\`. This is a personal, single-operator tool run by hand on one machine,
  not a shared or portable one — parameterizing the paths would add complexity with no one else to
  benefit from it.
- **One class per authoring session, not a general-purpose editor.** Each class corresponds to a
  specific real editing session (a course module, or a one-off correction like
  `EnglishForItB2Unit1ResetFirst10Ids`). Old classes are kept around after their run rather than
  deleted — they're a changelog of what was done to which archive, not dead code to clean up.
- **`Verify` and `verify`** (the `dotnet run -- verify <zip> <unit-file>` command) is the only
  read-only, non-destructive operation here — useful for sanity-checking an archive's card counts
  without risking a write.
- **No automated tests.** Every module class does real file I/O against a specific personal ZIP
  archive that only exists on the author's machine at a specific point in the authoring process —
  there's no meaningful way to unit-test "append these exact cards to this exact file" without
  either a real archive fixture (which would need to be recreated per module, since each one
  consumes the previous one's output as its own input) or testing `CourseZipEditor`/`CardBuilders`
  in isolation from what they're actually used for. Not pursued this round since it wasn't
  requested.
